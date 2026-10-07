#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Demos;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Application.Features.Results.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Read side shared by the timeline and insights queries: finds the match and its analysis row, loads the
/// stored timeline from object storage and maps it, resolving "us" from the stored team or (for older rows without
/// one) the roster and the recorded score.</summary>
public static class MatchTimelineLoader
{
	#region Public Methods

	/// <summary>Loads and maps the match's timeline, or a NotFound/Failure error.</summary>
	public static async Task<ErrorOr<MatchTimelineDto>> LoadAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		ILogger logger,
		Guid matchResultId,
		CancellationToken cancellationToken)
	{
		var loaded = await LoadRawAsync(dbContext, fileStorage, logger, matchResultId, cancellationToken);
		return loaded.IsError
			? loaded.Errors
			: MatchTimelineMapper.Map(loaded.Value.Stored, loaded.Value.OurTeam.ToList()) with { ExcludedPlayers = loaded.Value.Excluded };
	}

	/// <summary>Loads the stored timeline and resolves our team (stored, else roster/score), or a NotFound/Failure error.</summary>
	public static async Task<ErrorOr<LoadedMatchTimeline>> LoadRawAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		ILogger logger,
		Guid matchResultId,
		CancellationToken cancellationToken)
	{
		var result = await dbContext.MatchResults.AsNoTracking().FirstOrDefaultAsync(r => r.Id == matchResultId, cancellationToken);
		if (result is null)
		{
			return ResultErrors.MatchNotFound;
		}

		var analysis = await dbContext.MatchDemoAnalyses.AsNoTracking()
			.FirstOrDefaultAsync(a => a.MatchResultId == matchResultId, cancellationToken);
		if (analysis is null)
		{
			return MatchAnalysisErrors.TimelineNotFound;
		}

		if (!fileStorage.IsConfigured)
		{
			return ResultErrors.StorageNotConfigured;
		}

		StoredDemoTimeline stored;
		try
		{
			stored = await MatchTimelineStorage.LoadAsync(fileStorage, analysis.ObjectKey, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się wczytać osi czasu meczu {MatchResultId} (ObjectKey={ObjectKey})", matchResultId, analysis.ObjectKey);
			return MatchAnalysisErrors.TimelineUnavailable;
		}

		stored = stored with { Timeline = await PlayerNameResolver.ApplyAsync(dbContext, stored.Timeline, cancellationToken) };

		var names = TimelinePlayerNames.Collect(stored.Timeline);
		var excluded = analysis.ExcludedSteamIds
			.Select(id => new ExcludedPlayerDto(id.ToString(), names.TryGetValue(id, out var name) ? name : PlayerNameResolver.Fallback(id)))
			.ToList();
		stored = stored with { Timeline = RoundParticipants.Exclude(stored.Timeline, analysis.ExcludedSteamIds) };

		IReadOnlyList<long> ourTeam = analysis.OurTeamSteamIds;
		if (ourTeam.Count == 0)
		{
			var roster = await MatchTimelineAttacher.LoadRosterSteamIdsAsync(dbContext, cancellationToken);
			ourTeam = TimelineTeamResolver.ResolveOurTeam(stored.Timeline.Rounds, roster, result.OurScore, result.OpponentScore);
		}

		return new LoadedMatchTimeline(stored, ourTeam, excluded);
	}

	#endregion
}

/// <summary>A match's stored timeline plus the SteamID64s resolved as "us" (empty when unknown).</summary>
public record LoadedMatchTimeline(StoredDemoTimeline Stored, IReadOnlyList<long> OurTeam, IReadOnlyList<ExcludedPlayerDto>? ExcludedPlayers = null)
{
	/// <summary>The manually excluded players (never null).</summary>
	public IReadOnlyList<ExcludedPlayerDto> Excluded => ExcludedPlayers ?? [];
}
