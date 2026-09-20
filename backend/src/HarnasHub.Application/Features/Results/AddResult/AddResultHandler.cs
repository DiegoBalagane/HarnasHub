using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Handles <see cref="AddResultCommand"/> by persisting the new match result, deriving its score and map from
/// an attached demo when one was uploaded and falling back to the manually entered values otherwise.</summary>
public class AddResultHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier,
	IDemoParser demoParser)
	: IRequestHandler<AddResultCommand, ErrorOr<MatchResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MatchResultDto>> Handle(AddResultCommand request, CancellationToken cancellationToken)
	{
		var ourScore = request.OurScore;
		var opponentScore = request.OpponentScore;
		var mapName = request.MapName;

		if (request.DemoStream is not null)
		{
			var derived = await DeriveFromDemoAsync(request.DemoStream, cancellationToken);
			if (derived.IsError)
			{
				return derived.Errors;
			}

			// The demo only overrides what it could actually tell us — an unattributable score or an unrecognised
			// map leaves whatever the coach typed in place.
			mapName = derived.Value.MapName ?? mapName;
			if (derived.Value.Score is { } score)
			{
				ourScore = score.OurScore;
				opponentScore = score.OpponentScore;
			}
		}

		if (ourScore is not { } resolvedOurScore || opponentScore is not { } resolvedOpponentScore)
		{
			return ResultErrors.ScoreRequired;
		}

		var result = new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = request.Opponent,
			OurScore = resolvedOurScore,
			OpponentScore = resolvedOpponentScore,
			MapName = mapName,
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
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("stats", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

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

	/// <summary>Parses the uploaded demo in memory (it is never persisted) and reads the map plus the score, the latter
	/// only when at least one round has a roster member on one of its sides.</summary>
	private async Task<ErrorOr<DerivedFromDemo>> DeriveFromDemoAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		DemoParseResult parsed;

		try
		{
			parsed = await demoParser.ParseAsync(demoStream, cancellationToken);
		}
		catch (Exception)
		{
			// Any parser failure (corrupt file, unsupported build, wrong file type) is a validation problem for the
			// caller, not a server error — the demo bytes themselves are untrusted input.
			return ResultErrors.InvalidDemoFile;
		}

		if (parsed.RoundsPlayed == 0)
		{
			return ResultErrors.InvalidDemoFile;
		}

		var rosterSteamIds = await ResolveRosterSteamIdsAsync(cancellationToken);
		var score = DemoScoreCalculator.Calculate(parsed.Rounds, rosterSteamIds);

		return new DerivedFromDemo(parsed.MapName?.ToString(), score);
	}

	/// <summary>Loads every roster member who has told us their SteamID64 — the set the demo's rounds are matched against.</summary>
	private async Task<IReadOnlySet<long>> ResolveRosterSteamIdsAsync(CancellationToken cancellationToken)
	{
		// Steam IDs are stored as strings (see User.SteamId64) because they exceed Number.MAX_SAFE_INTEGER on the way
		// to the browser; the roster is small enough to pull whole and convert here rather than filter by the demo.
		var steamIds = await dbContext.Users
			.Where(u => u.SteamId64 != null)
			.Select(u => u.SteamId64!)
			.ToListAsync(cancellationToken);

		return steamIds
			.Select(id => long.TryParse(id, out var parsed) ? parsed : (long?)null)
			.Where(id => id.HasValue)
			.Select(id => id!.Value)
			.ToHashSet();
	}

	#endregion

	#region Private Types

	/// <summary>What a parsed demo could contribute to the result — either part may be null when the demo didn't say.</summary>
	private record DerivedFromDemo(string? MapName, (int OurScore, int OpponentScore)? Score);

	#endregion
}
