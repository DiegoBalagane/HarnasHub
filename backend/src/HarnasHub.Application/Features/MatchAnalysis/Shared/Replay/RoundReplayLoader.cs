#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Demos;
using HarnasHub.Application.Features.OpponentReport.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;

/// <summary>Loads a stored timeline (one of our matches or an analysed opponent demo) and maps one of its rounds into a
/// replay — shared by both replay queries and the "board from round" command.</summary>
public static class RoundReplayLoader
{
	#region Public Methods

	/// <summary>Replay of round <paramref name="roundNumber"/> of one of our matches, from our perspective.</summary>
	public static async Task<ErrorOr<RoundReplayDto>> ForMatchAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		ILogger logger,
		Guid matchResultId,
		int roundNumber,
		CancellationToken cancellationToken)
	{
		var loaded = await MatchTimelineLoader.LoadRawAsync(dbContext, fileStorage, logger, matchResultId, cancellationToken);
		if (loaded.IsError)
		{
			return loaded.Errors;
		}

		var opponent = await dbContext.MatchResults.AsNoTracking()
			.Where(r => r.Id == matchResultId)
			.Select(r => r.Opponent)
			.FirstOrDefaultAsync(cancellationToken);

		var perspective = new ReplayPerspective(loaded.Value.OurTeam, [], opponent);
		var replay = RoundReplayMapper.Build(loaded.Value.Stored.Timeline, roundNumber, perspective);
		return replay is null ? MatchAnalysisErrors.RoundNotFound : replay;
	}

	/// <summary>Replay of round <paramref name="roundNumber"/> of an analysed opponent demo; our roster (if it played)
	/// is labelled ours, the detected opponent theirs.</summary>
	public static async Task<ErrorOr<RoundReplayDto>> ForOpponentDemoAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		ILogger logger,
		Guid opponentDemoId,
		int roundNumber,
		CancellationToken cancellationToken)
	{
		var analysis = await dbContext.OpponentDemoAnalyses.AsNoTracking().FirstOrDefaultAsync(a => a.Id == opponentDemoId, cancellationToken);
		if (analysis is null)
		{
			return OpponentDemoErrors.NotFound;
		}

		if (!fileStorage.IsConfigured)
		{
			return OpponentDemoErrors.StorageNotConfigured;
		}

		StoredDemoTimeline stored;
		try
		{
			stored = await MatchTimelineStorage.LoadAsync(fileStorage, analysis.TimelineObjectKey, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się wczytać osi czasu demki rywala {DemoId} (ObjectKey={ObjectKey})", opponentDemoId, analysis.TimelineObjectKey);
			return OpponentDemoErrors.TimelineUnavailable;
		}

		stored = stored with { Timeline = await PlayerNameResolver.ApplyAsync(dbContext, stored.Timeline, cancellationToken) };

		var roster = await MatchTimelineAttacher.LoadRosterSteamIdsAsync(dbContext, cancellationToken);
		var ours = analysis.TeamASteamIds.Concat(analysis.TeamBSteamIds)
			.Where(id => roster.Contains(id) && !analysis.OpponentSteamIds.Contains(id))
			.ToList();

		var displayName = await dbContext.OpponentFaceitLinks.AsNoTracking()
			.Where(l => l.OpponentKey == analysis.OpponentKey)
			.Select(l => l.DisplayName)
			.FirstOrDefaultAsync(cancellationToken);

		var perspective = new ReplayPerspective(ours, analysis.OpponentSteamIds, string.IsNullOrWhiteSpace(displayName) ? analysis.OpponentKey : displayName);
		var replay = RoundReplayMapper.Build(stored.Timeline, roundNumber, perspective);
		return replay is null ? MatchAnalysisErrors.RoundNotFound : replay;
	}

	#endregion
}
