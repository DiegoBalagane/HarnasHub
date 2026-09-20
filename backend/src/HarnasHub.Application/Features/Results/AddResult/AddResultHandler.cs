using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Application.Features.Stats.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Handles <see cref="AddResultCommand"/> by persisting the new match result and, when the coach analysed a
/// demo beforehand, a stat line for every demo participant who matches a roster member's SteamID64.</summary>
public class AddResultHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<AddResultCommand, ErrorOr<MatchResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MatchResultDto>> Handle(AddResultCommand request, CancellationToken cancellationToken)
	{
		if (request.OurScore is not { } resolvedOurScore || request.OpponentScore is not { } resolvedOpponentScore)
		{
			return ResultErrors.ScoreRequired;
		}

		var result = new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = request.Opponent,
			OurScore = resolvedOurScore,
			OpponentScore = resolvedOpponentScore,
			MapName = request.MapName,
			DemoUrl = request.DemoUrl,
			Notes = request.Notes,
			PlayedAtUtc = request.PlayedAtUtc,
			Category = request.Category,
			TournamentId = request.TournamentId,
			LeagueId = request.LeagueId,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.MatchResults.Add(result);

		var importedStatCount = 0;
		if (request.DemoPlayers is { Count: > 0 } demoPlayers
			&& request.DemoRoundsPlayed is { } roundsPlayed and > 0
			&& request.OurTeamSteamIds is { Count: > 0 } ourTeamSteamIds)
		{
			var ourPlayers = demoPlayers.Where(p => ourTeamSteamIds.Contains(p.SteamId64)).ToList();
			importedStatCount = await ImportTeamStatsAsync(result.Id, ourPlayers, roundsPlayed, cancellationToken);
		}

		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("stats", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);
		if (importedStatCount > 0)
		{
			await realtimeNotifier.NotifyAsync($"match-stats:{result.Id}", cancellationToken);
		}

		var tournament = request.TournamentId is null
			? null
			: await dbContext.Tournaments.FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);
		var league = request.LeagueId is null
			? null
			: await dbContext.Leagues.FirstOrDefaultAsync(l => l.Id == request.LeagueId, cancellationToken);

		return new MatchResultDto(
			result.Id,
			result.Opponent,
			result.OurScore,
			result.OpponentScore,
			result.MapName,
			result.DemoUrl,
			result.Notes,
			result.PlayedAtUtc,
			result.Category,
			tournament?.Id,
			tournament?.Name,
			league?.Id,
			league?.Name,
			league?.Season,
			league?.Type);
	}

	#endregion

	#region Private Methods

	/// <summary>Saves a stat line for every player on the coach-picked team — connected to a roster account when their
	/// SteamID64 matches one, and left unconnected (identified only by <see cref="PlayerMatchStat.DemoPlayerName"/>)
	/// otherwise, since a real teammate without their SteamID64 on file yet still deserves a stat line rather than
	/// being silently skipped. Returns how many rows were queued so the caller knows whether a per-match realtime
	/// notify is worth sending.</summary>
	private async Task<int> ImportTeamStatsAsync(
		Guid matchResultId,
		IReadOnlyList<AnalyzedDemoPlayerDto> players,
		int roundsPlayed,
		CancellationToken cancellationToken)
	{
		if (players.Count == 0)
		{
			return 0;
		}

		var steamIds = players.Select(p => p.SteamId64).ToList();
		var matchedUsers = await dbContext.Users
			.Where(u => u.SteamId64 != null && steamIds.Contains(u.SteamId64))
			.ToDictionaryAsync(u => u.SteamId64!, cancellationToken);

		var imported = 0;

		foreach (var player in players)
		{
			matchedUsers.TryGetValue(player.SteamId64, out var user);

			var demoPlayer = new DemoPlayerStats(
				long.Parse(player.SteamId64),
				player.DemoPlayerName,
				player.Kills,
				player.Deaths,
				player.Assists,
				player.Headshots,
				player.DamageDealt,
				player.EntryKills,
				player.EntryDeaths,
				player.KastRounds,
				player.UtilityDamage,
				player.FlashAssists,
				new Dictionary<int, int> { [2] = player.MultiKill2K, [3] = player.MultiKill3K, [4] = player.MultiKill4K, [5] = player.MultiKill5K },
				[]);
			var computed = PlayerStatCalculator.Compute(demoPlayer, roundsPlayed);

			dbContext.PlayerMatchStats.Add(new PlayerMatchStat
			{
				Id = Guid.NewGuid(),
				MatchResultId = matchResultId,
				UserId = user?.Id,
				DemoPlayerName = user is null ? player.DemoPlayerName : null,
				Kills = player.Kills,
				Deaths = player.Deaths,
				Assists = player.Assists,
				Adr = computed.Adr,
				HeadshotPercentage = computed.HeadshotPercentage,
				Rating = computed.Rating,
				EntryKills = player.EntryKills,
				EntryDeaths = player.EntryDeaths,
				KastPercentage = computed.KastPercentage,
				MultiKill2K = player.MultiKill2K,
				MultiKill3K = player.MultiKill3K,
				MultiKill4K = player.MultiKill4K,
				MultiKill5K = player.MultiKill5K,
				UtilityDamage = player.UtilityDamage,
				FlashAssists = player.FlashAssists,
				CreatedAtUtc = DateTime.UtcNow
			});

			imported++;
		}

		return imported;
	}

	#endregion
}
