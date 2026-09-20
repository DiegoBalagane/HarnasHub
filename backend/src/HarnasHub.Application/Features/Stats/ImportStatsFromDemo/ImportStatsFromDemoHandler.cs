using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Stats.ImportStatsFromDemo;

/// <summary>Handles <see cref="ImportStatsFromDemoCommand"/> by delegating the byte-level parsing to <see cref="IDemoParser"/>
/// and turning the raw totals into ADR/HS%/an approximate rating, matching each player to a roster account by SteamID64.</summary>
public class ImportStatsFromDemoHandler(IDemoParser demoParser, IApplicationDbContext dbContext)
	: IRequestHandler<ImportStatsFromDemoCommand, ErrorOr<ImportStatsFromDemoResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<ImportStatsFromDemoResultDto>> Handle(ImportStatsFromDemoCommand request, CancellationToken cancellationToken)
	{
		DemoParseResult parsed;

		try
		{
			parsed = await demoParser.ParseAsync(request.DemoStream, cancellationToken);
		}
		catch (Exception)
		{
			// Any parser failure (corrupt file, unsupported build, wrong file type) is a validation problem for the
			// caller, not a server error — the demo bytes themselves are untrusted input.
			return StatsErrors.InvalidDemoFile;
		}

		if (parsed.RoundsPlayed == 0 || parsed.Players.Count == 0)
		{
			return StatsErrors.InvalidDemoFile;
		}

		// Steam IDs travel as strings end-to-end (see User.SteamId64) — they exceed Number.MAX_SAFE_INTEGER,
		// so comparing/keying on the numeric form here would risk the same precision loss JSON is avoided for.
		var steamIds = parsed.Players.Select(p => p.SteamId64.ToString()).ToList();
		var matchedUsers = await dbContext.Users
			.Where(u => u.SteamId64 != null && steamIds.Contains(u.SteamId64))
			.ToDictionaryAsync(u => u.SteamId64!, cancellationToken);

		var players = parsed.Players
			.Select(player =>
			{
				matchedUsers.TryGetValue(player.SteamId64.ToString(), out var user);

				var adr = Math.Round((double)player.DamageDealt / parsed.RoundsPlayed, 1);
				var headshotPercentage = player.Kills == 0 ? 0 : Math.Round((double)player.Headshots / player.Kills * 100, 1);
				var kastPercentage = Math.Round((double)player.KastRounds / parsed.RoundsPlayed * 100, 1);
				var rating = ApproximateRating(player, parsed.RoundsPlayed, adr, kastPercentage);

				return new ParsedPlayerStatDto(
					player.SteamId64.ToString(),
					player.PlayerName,
					user?.Id,
					user is null ? null : user.InGameNickname ?? user.DisplayName,
					player.Kills,
					player.Deaths,
					player.Assists,
					adr,
					headshotPercentage,
					rating,
					player.EntryKills,
					player.EntryDeaths,
					kastPercentage,
					player.MultiKillRounds.GetValueOrDefault(2),
					player.MultiKillRounds.GetValueOrDefault(3),
					player.MultiKillRounds.GetValueOrDefault(4),
					player.MultiKillRounds.GetValueOrDefault(5),
					player.UtilityDamage,
					player.FlashAssists,
					player.DeathPositions.Select(d => new DeathPositionDto(d.X, d.Y, d.Side.ToString())).ToList());
			})
			.OrderByDescending(p => p.Rating)
			.ToList();

		return new ImportStatsFromDemoResultDto(parsed.RoundsPlayed, parsed.MapName?.ToString(), players);
	}

	#endregion

	#region Private Methods

	/// <summary>A deliberately simple stand-in for HLTV's Rating 2.0 — kills/deaths/assists per round, a damage term,
	/// and (now that KAST is tracked) a small KAST bonus, clamped at 0. Coach/Manager edits this before saving, so it
	/// only needs to be a reasonable starting point, not exact.</summary>
	private static double ApproximateRating(DemoPlayerStats player, int roundsPlayed, double adr, double kastPercentage)
	{
		var killsPerRound = (double)player.Kills / roundsPlayed;
		var deathsPerRound = (double)player.Deaths / roundsPlayed;
		var assistsPerRound = (double)player.Assists / roundsPlayed;

		var rating = killsPerRound * 0.45 + assistsPerRound * 0.15 - deathsPerRound * 0.3 + adr / 100 * 0.25 + kastPercentage / 100 * 0.15;

		return Math.Round(Math.Max(rating, 0), 2);
	}

	#endregion
}
