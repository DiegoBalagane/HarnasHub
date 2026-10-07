#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Stores a parsed timeline as a match's timeline: uploads it to the match's key and creates/updates its
/// <see cref="MatchDemoAnalysis"/> row. Shared by adding a result from an analysed demo and attaching a demo later.</summary>
public static class MatchTimelineAttacher
{
	#region Public Methods

	/// <summary>Uploads the timeline and upserts the index row (saving changes) — re-attaching replaces the old timeline.</summary>
	public static async Task<MatchDemoAnalysis> AttachAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		Guid matchResultId,
		DemoTimeline timeline,
		int parserVersion,
		IReadOnlyList<long> ourTeamSteamIds,
		CancellationToken cancellationToken)
	{
		var objectKey = MatchTimelineStorage.MatchKey(matchResultId);
		await MatchTimelineStorage.SaveAsync(fileStorage, objectKey, timeline, parserVersion, cancellationToken);

		var analysis = await dbContext.MatchDemoAnalyses.FirstOrDefaultAsync(a => a.MatchResultId == matchResultId, cancellationToken);
		if (analysis is null)
		{
			analysis = new MatchDemoAnalysis { Id = Guid.NewGuid(), MatchResultId = matchResultId };
			dbContext.MatchDemoAnalyses.Add(analysis);
		}

		analysis.ObjectKey = objectKey;
		analysis.ParserVersion = parserVersion;
		analysis.RoundsCount = timeline.Rounds.Count;
		analysis.OurTeamSteamIds = ourTeamSteamIds.ToList();
		analysis.CreatedAtUtc = DateTime.UtcNow;

		await RefreshDeathPositionsAsync(dbContext, matchResultId, timeline, cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);
		return analysis;
	}

	/// <summary>Moves a parked (pending) timeline to <paramref name="result"/>; "us" comes from the coach-picked SteamIDs,
	/// else the roster/score. Never throws for storage problems — logs and returns false — and deletes the pending
	/// object either way (the pending-timeline cleanup job sweeps anything this misses).</summary>
	public static async Task<bool> ClaimPendingAsync(
		IApplicationDbContext dbContext,
		IFileStorage fileStorage,
		ILogger logger,
		MatchResult result,
		string pendingKey,
		IReadOnlyList<string>? ourTeamSteamIds,
		CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return false;
		}

		try
		{
			var stored = await MatchTimelineStorage.LoadAsync(fileStorage, pendingKey, cancellationToken);

			IReadOnlyList<long> ourTeam = (ourTeamSteamIds ?? [])
				.Select(id => long.TryParse(id, out var parsed) ? parsed : (long?)null)
				.OfType<long>()
				.ToList();
			if (ourTeam.Count == 0)
			{
				var roster = await LoadRosterSteamIdsAsync(dbContext, cancellationToken);
				ourTeam = TimelineTeamResolver.ResolveOurTeam(stored.Timeline.Rounds, roster, result.OurScore, result.OpponentScore);
			}

			await AttachAsync(dbContext, fileStorage, result.Id, stored.Timeline, stored.ParserVersion, ourTeam, cancellationToken);
			return true;
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się dołączyć osi czasu do meczu {MatchResultId} (PendingKey={PendingKey})", result.Id, pendingKey);
			return false;
		}
		finally
		{
			try
			{
				await fileStorage.DeleteAsync(pendingKey, CancellationToken.None);
			}
			catch (Exception)
			{
				// Best-effort — the cleanup job removes stale pending timelines.
			}
		}
	}

	/// <summary>Re-syncs the match's existing stat rows' death positions with the timeline's (current calibration) — only that field.</summary>
	private static async Task RefreshDeathPositionsAsync(
		IApplicationDbContext dbContext, Guid matchResultId, DemoTimeline timeline, CancellationToken cancellationToken)
	{
		if (timeline.Stats is not { Players.Count: > 0 } stats)
		{
			return;
		}

		var rows = await dbContext.PlayerMatchStats.Where(s => s.MatchResultId == matchResultId).ToListAsync(cancellationToken);
		if (rows.Count == 0)
		{
			return;
		}

		var userIds = rows.Where(r => r.UserId != null).Select(r => r.UserId!.Value).ToList();
		var users = await dbContext.Users
			.Where(u => userIds.Contains(u.Id) && u.SteamId64 != null)
			.Select(u => new { u.Id, u.SteamId64 })
			.ToListAsync(cancellationToken);
		var steamIdByUserId = users
			.Where(u => long.TryParse(u.SteamId64, out _))
			.ToDictionary(u => u.Id, u => long.Parse(u.SteamId64!));

		DeathPositionRefresher.Refresh(rows, steamIdByUserId, stats.Players);
	}

	/// <summary>Every roster member's SteamID64 that parses as a number — the default way to recognise "us" in a demo.</summary>
	public static async Task<HashSet<long>> LoadRosterSteamIdsAsync(IApplicationDbContext dbContext, CancellationToken cancellationToken)
	{
		var steamIds = await dbContext.Users
			.Where(u => u.SteamId64 != null)
			.Select(u => u.SteamId64!)
			.ToListAsync(cancellationToken);

		return steamIds
			.Select(id => long.TryParse(id, out var parsed) ? parsed : (long?)null)
			.OfType<long>()
			.ToHashSet();
	}

	#endregion
}
