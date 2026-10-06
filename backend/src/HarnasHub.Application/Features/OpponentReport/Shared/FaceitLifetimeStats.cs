using System.Text.Json;
using HarnasHub.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Caches each player's lifetime per-map FACEIT stats on <c>FaceitPlayer.MapStatsJson</c> and reads them back.</summary>
public static class FaceitLifetimeStats
{
	#region Private Fields

	private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

	#endregion

	#region Public Methods

	/// <summary>Refreshes the lifetime map stats of the given players whose cache is older than <see cref="FaceitSync.ProfileMaxAge"/>
	/// (the profile cadence). A player FACEIT fails for is skipped — lifetime numbers are a low-weight extra, never worth failing a sync.</summary>
	public static async Task RefreshAsync(
		IApplicationDbContext dbContext,
		IFaceitClient client,
		IReadOnlyCollection<string> playerIds,
		DateTime nowUtc,
		CancellationToken cancellationToken)
	{
		foreach (var playerId in playerIds.Distinct())
		{
			var player = await dbContext.FaceitPlayers.FindAsync([playerId], cancellationToken);
			if (player is null || player.MapStatsSyncedAtUtc >= nowUtc - FaceitSync.ProfileMaxAge)
			{
				continue;
			}

			try
			{
				var maps = await client.GetPlayerMapStatsAsync(playerId, cancellationToken);
				player.MapStatsJson = Serialize(maps);
				player.MapStatsSyncedAtUtc = nowUtc;
			}
			catch (HttpRequestException)
			{
				// Keep the previous numbers; the next sync retries.
			}
		}

		await dbContext.SaveChangesAsync(cancellationToken);
	}

	/// <summary>Player id → lifetime map stats for the given ids, from the cache; players without cached stats are left out.</summary>
	public static async Task<Dictionary<string, IReadOnlyList<FaceitLifetimeMapStats>>> LoadAsync(
		IApplicationDbContext dbContext,
		IReadOnlyCollection<string> playerIds,
		CancellationToken cancellationToken)
	{
		var rows = await dbContext.FaceitPlayers.AsNoTracking()
			.Where(p => playerIds.Contains(p.Id) && p.MapStatsJson != null)
			.Select(p => new { p.Id, p.MapStatsJson })
			.ToListAsync(cancellationToken);

		return rows.ToDictionary(r => r.Id, r => Deserialize(r.MapStatsJson));
	}

	/// <summary>The JSON stored on the player.</summary>
	public static string Serialize(IReadOnlyList<FaceitLifetimeMapStats> maps) => JsonSerializer.Serialize(maps, JsonOptions);

	/// <summary>Parses stored JSON; empty for null or JSON that no longer fits.</summary>
	public static IReadOnlyList<FaceitLifetimeMapStats> Deserialize(string? json)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			return [];
		}

		try
		{
			return JsonSerializer.Deserialize<List<FaceitLifetimeMapStats>>(json, JsonOptions) ?? [];
		}
		catch (JsonException)
		{
			return [];
		}
	}

	#endregion
}
