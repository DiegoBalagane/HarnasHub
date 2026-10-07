using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Pulls FACEIT data into the local cache: player profiles, match history and per-map scoreboards. Matches already
/// cached are never fetched again, and a single run is capped so a manual refresh stays within a reasonable request time.</summary>
public static class FaceitSync
{
	#region Public Fields

	/// <summary>How far back match history is pulled and reported on.</summary>
	public static readonly TimeSpan HistoryWindow = TimeSpan.FromDays(120);

	/// <summary>Most matchmaking history entries per player and run whose scoreboards are fetched; championship/hub rooms are
	/// always taken from the whole paged history (see <see cref="MaxHistoryPages"/>).</summary>
	public const int HistoryLimit = 50;

	/// <summary>FACEIT's maximum history page size.</summary>
	public const int HistoryPageSize = 100;

	/// <summary>How many history pages per player are read at most — paging stops as soon as the window is exhausted, so this only
	/// bites for very busy PUG players and keeps a whole ESEA season of league games reachable behind their matchmaking.</summary>
	public const int MaxHistoryPages = 10;

	/// <summary>A player's history pulled more recently than this is skipped — dedupes our roster across several opponent syncs.</summary>
	public static readonly TimeSpan PlayerResyncInterval = TimeSpan.FromMinutes(10);

	/// <summary>A cached profile (elo, nickname) older than this is refreshed.</summary>
	public static readonly TimeSpan ProfileMaxAge = TimeSpan.FromDays(1);

	/// <summary>Most new matches fetched per run; the rest follows on the next sync.</summary>
	public const int MaxNewMatchesPerRun = 120;

	#endregion

	#region Public Methods

	/// <summary>Inserts or updates a cached player; null values never overwrite known ones. The caller saves changes.</summary>
	public static async Task<FaceitPlayer> UpsertPlayerAsync(
		IApplicationDbContext dbContext,
		string playerId,
		string nickname,
		string? steamId64,
		int? elo,
		int? skillLevel,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		var player = await dbContext.FaceitPlayers.FindAsync([playerId], cancellationToken);
		if (player is null)
		{
			player = new FaceitPlayer { Id = playerId };
			dbContext.FaceitPlayers.Add(player);
		}

		player.Nickname = nickname;
		player.SteamId64 = steamId64 ?? player.SteamId64;
		player.Elo = elo ?? player.Elo;
		player.SkillLevel = skillLevel ?? player.SkillLevel;
		player.UpdatedAtUtc = nowUtc;
		return player;
	}

	/// <summary>Our roster's FACEIT ids, resolved from each shown-in-stats user's SteamID64, then their manual FACEIT nickname
	/// (see <see cref="OurRosterFaceit"/>); cached profiles are reused while fresh.</summary>
	public static Task<List<string>> ResolveOurPlayersAsync(
		IApplicationDbContext dbContext,
		IFaceitClient client,
		DateTime nowUtc,
		CancellationToken cancellationToken) =>
		OurRosterFaceit.ResolveAsync(dbContext, client, nowUtc, cancellationToken);

	/// <summary>Refreshes stale cached profiles (elo, nickname) of the given players.</summary>
	public static async Task RefreshProfilesAsync(
		IApplicationDbContext dbContext,
		IFaceitClient client,
		IReadOnlyCollection<string> playerIds,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		foreach (var playerId in playerIds.Distinct())
		{
			var player = await dbContext.FaceitPlayers.FindAsync([playerId], cancellationToken);
			if (player is not null && player.UpdatedAtUtc >= nowUtc - ProfileMaxAge && player.Elo.HasValue)
			{
				continue;
			}

			var info = await client.GetPlayerAsync(playerId, cancellationToken);
			if (info is not null)
			{
				await UpsertPlayerAsync(dbContext, info.PlayerId, info.Nickname, info.SteamId64, info.Elo, info.SkillLevel, nowUtc, cancellationToken);
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
	}

	/// <summary>Caches the finished matches of each player within <see cref="HistoryWindow"/> that aren't cached yet and fills the
	/// faction/competition ids of cached ones that miss them (from the same history pages, no extra call);
	/// <paramref name="onProgress"/> receives the fraction of players processed, so a job's bar moves during this slowest step.</summary>
	public static async Task<(int NewMapGames, bool Complete)> SyncHistoryAsync(
		IApplicationDbContext dbContext,
		IFaceitClient client,
		IReadOnlyCollection<string> playerIds,
		DateTime nowUtc,
		CancellationToken cancellationToken,
		Action<double>? onProgress = null)
	{
		var distinctIds = playerIds.Distinct().ToList();
		var processed = 0;
		var since = nowUtc - HistoryWindow;
		var known = (await dbContext.FaceitMatches
				.Where(m => m.PlayedAtUtc >= since.AddDays(-1))
				.Select(m => m.FaceitMatchId)
				.ToListAsync(cancellationToken))
			.ToHashSet();
		var withoutFactions = (await dbContext.FaceitMatches
				.Where(m => m.PlayedAtUtc >= since.AddDays(-1) && (m.Team1FactionId == null || m.Team2FactionId == null || m.CompetitionId == null))
				.Select(m => m.FaceitMatchId)
				.ToListAsync(cancellationToken))
			.ToHashSet();
		var newMapGames = 0;
		var fetched = 0;

		foreach (var playerId in distinctIds)
		{
			onProgress?.Invoke(processed++ / (double)distinctIds.Count);

			var player = await dbContext.FaceitPlayers.FindAsync([playerId], cancellationToken);
			if (player?.HistorySyncedAtUtc >= nowUtc - PlayerResyncInterval)
			{
				continue;
			}

			var history = await FaceitHistoryPager.LoadAsync(client, playerId, since, cancellationToken);
			await BackfillAsync(dbContext, history.Where(h => withoutFactions.Remove(h.MatchId)).ToList(), cancellationToken);
			foreach (var item in FaceitHistoryPager.SelectToFetch(history).Where(h => IsFinished(h.Status) && !known.Contains(h.MatchId)))
			{
				if (fetched >= MaxNewMatchesPerRun)
				{
					await dbContext.SaveChangesAsync(cancellationToken);
					return (newMapGames, false);
				}

				var maps = await client.GetMatchStatsAsync(item.MatchId, cancellationToken);
				fetched++;
				known.Add(item.MatchId);
				newMapGames += FaceitMatchRows.Add(dbContext, item, maps, nowUtc);
			}

			if (player is not null)
			{
				player.HistorySyncedAtUtc = nowUtc;
			}

			await dbContext.SaveChangesAsync(cancellationToken);
		}

		return (newMapGames, true);
	}

	#endregion

	#region Private Methods

	/// <summary>Fills faction/competition ids of the cached rows of <paramref name="items"/> (already cached matches).</summary>
	private static async Task BackfillAsync(IApplicationDbContext dbContext, List<FaceitHistoryItem> items, CancellationToken cancellationToken)
	{
		if (items.Count == 0)
		{
			return;
		}

		var ids = items.Select(i => i.MatchId).ToList();
		var rows = await dbContext.FaceitMatches.Where(m => ids.Contains(m.FaceitMatchId)).ToListAsync(cancellationToken);
		foreach (var item in items)
		{
			FaceitMatchRows.Backfill(rows, item);
		}
	}

	/// <summary>History entries without a status are kept; anything else must be FINISHED (cancelled rooms have no stats).</summary>
	private static bool IsFinished(string? status) =>
		string.IsNullOrWhiteSpace(status) || status.Equals("FINISHED", StringComparison.OrdinalIgnoreCase);

	#endregion
}
