#region Usings

using System.Text.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Stats.Shared;
using HarnasHub.Core.Entities;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Pure matching of a match's stat rows to a freshly attached timeline's players, so the stored death positions
/// follow the timeline's current radar calibration without touching any other stat of the row.</summary>
public static class DeathPositionRefresher
{
	#region Public Methods

	/// <summary>Overwrites <see cref="PlayerMatchStat.DeathPositionsJson"/> (only) of every row that matches a timeline player —
	/// by the linked user's SteamID64 first, then by <see cref="PlayerMatchStat.DemoPlayerName"/> (case-insensitive); returns the
	/// number of rows refreshed. Unmatched rows stay unchanged.</summary>
	public static int Refresh(
		IEnumerable<PlayerMatchStat> rows,
		IReadOnlyDictionary<Guid, long> steamIdByUserId,
		IReadOnlyList<DemoPlayerStats> players)
	{
		var refreshed = 0;
		foreach (var row in rows)
		{
			var player = FindPlayer(row, steamIdByUserId, players);
			if (player is null)
			{
				continue;
			}

			row.DeathPositionsJson = player.DeathPositions.Count == 0
				? null
				: JsonSerializer.Serialize(player.DeathPositions.Select(d => new DeathPositionDto(d.X, d.Y, d.Side.ToString())).ToList());
			refreshed++;
		}

		return refreshed;
	}

	#endregion

	#region Private Methods

	private static DemoPlayerStats? FindPlayer(
		PlayerMatchStat row,
		IReadOnlyDictionary<Guid, long> steamIdByUserId,
		IReadOnlyList<DemoPlayerStats> players)
	{
		if (row.UserId is { } userId && steamIdByUserId.TryGetValue(userId, out var steamId))
		{
			var bySteam = players.FirstOrDefault(p => p.SteamId64 == steamId);
			if (bySteam is not null)
			{
				return bySteam;
			}
		}

		return string.IsNullOrWhiteSpace(row.DemoPlayerName)
			? null
			: players.FirstOrDefault(p => string.Equals(p.PlayerName, row.DemoPlayerName, StringComparison.OrdinalIgnoreCase));
	}

	#endregion
}
