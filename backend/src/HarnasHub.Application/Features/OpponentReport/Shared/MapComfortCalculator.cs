using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>A roster's individual comfort on one map from its players' solo games. Fractions (0–1): <paramref name="SoloShare"/> is
/// the average share of a rated player's solo pool games on this map; <paramref name="AvgWinRate"/>, <paramref name="AvgSmoothedWinRate"/>
/// and <paramref name="AvgKdRatio"/> average the regulars only (null without regulars).</summary>
public record MapComfort(
	MapName Map,
	int RatedPlayers,
	int RegularPlayers,
	int AvoidingPlayers,
	double SoloShare,
	double? AvgWinRate,
	double? AvgSmoothedWinRate,
	double? AvgKdRatio,
	List<string> RegularNicknames,
	List<string> AvoidingNicknames)
{
	/// <summary>Most rated players avoid the map solo and at most one plays it regularly — "they don't play it, even alone".</summary>
	public bool IsAvoided => AvoidingPlayers >= 2 && AvoidingPlayers * 2 > RatedPlayers && RegularPlayers <= 1;

	/// <summary>Most rated players play the map regularly solo and win at least half of it on average.</summary>
	public bool IsComfortable => RegularPlayers >= 2 && RegularPlayers * 2 > RatedPlayers && AvgWinRate >= 0.5;
}

/// <summary>Computes <see cref="MapComfort"/> per pool map. Only solo games count — team games are already in the team metrics, so
/// using them here would count the same evidence twice.</summary>
public static class MapComfortCalculator
{
	#region Public Fields

	/// <summary>A player needs this many solo pool games in the window to be judged at all (else "avoids everything" is just inactivity).</summary>
	public const int MinSoloGamesToRate = 10;

	/// <summary>This many solo games on a map make a player a regular on it.</summary>
	public const int RegularGames = 3;

	/// <summary>At most this many solo games on a map count as avoiding it.</summary>
	public const int AvoidingGames = 1;

	#endregion

	#region Public Methods

	/// <summary>Comfort for every pool map; all zeros when no player has enough solo games.</summary>
	public static Dictionary<MapName, MapComfort> Calculate(IEnumerable<IndividualGameLine> lines)
	{
		var rated = lines
			.Where(l => !l.TeamGame && l.Map.HasValue)
			.GroupBy(l => l.PlayerId)
			.Select(g => g.OrderBy(l => l.PlayedAtUtc).ToList())
			.Where(g => g.Count >= MinSoloGamesToRate)
			.ToList();

		return Enum.GetValues<MapName>().ToDictionary(map => map, map => ForMap(map, rated));
	}

	/// <summary>The DTO form: fractions as percentages, only maps when someone is rated.</summary>
	public static List<MapComfortDto> ToDtos(IReadOnlyDictionary<MapName, MapComfort> comfort) =>
		comfort.Values
			.Where(c => c.RatedPlayers > 0)
			.OrderByDescending(c => c.RegularPlayers)
			.ThenBy(c => c.AvoidingPlayers)
			.ThenBy(c => c.Map)
			.Select(c => new MapComfortDto(
				c.Map.ToString(),
				c.RatedPlayers,
				c.RegularPlayers,
				c.AvoidingPlayers,
				Math.Round(c.SoloShare * 100, 1),
				c.AvgWinRate is { } wr ? Math.Round(wr * 100, 1) : null,
				c.AvgKdRatio is { } kd ? Math.Round(kd, 2) : null,
				c.RegularNicknames,
				c.AvoidingNicknames))
			.ToList();

	#endregion

	#region Private Methods

	/// <summary>One map's comfort from the rated players' solo lines (each list oldest first, one player per list).</summary>
	private static MapComfort ForMap(MapName map, List<List<IndividualGameLine>> rated)
	{
		var perPlayer = rated
			.Select(all => (All: all, OnMap: all.Where(l => l.Map == map).ToList(), Nickname: all[^1].Nickname))
			.ToList();
		var regulars = perPlayer.Where(p => p.OnMap.Count >= RegularGames).ToList();
		var avoiding = perPlayer.Where(p => p.OnMap.Count <= AvoidingGames).ToList();

		return new MapComfort(
			map,
			perPlayer.Count,
			regulars.Count,
			avoiding.Count,
			perPlayer.Count == 0 ? 0 : perPlayer.Average(p => (double)p.OnMap.Count / p.All.Count),
			regulars.Count == 0 ? null : regulars.Average(p => (double)p.OnMap.Count(l => l.Won) / p.OnMap.Count),
			regulars.Count == 0 ? null : regulars.Average(p => MapAdvantage.SmoothedWinRate(p.OnMap.Count(l => l.Won), p.OnMap.Count)),
			regulars.Count == 0 ? null : regulars.Average(p => PlayerFormCalculator.Kd(p.OnMap)),
			Names(regulars.Select(p => p.Nickname)),
			Names(avoiding.Select(p => p.Nickname)));
	}

	/// <summary>Nicknames sorted case-insensitively.</summary>
	private static List<string> Names(IEnumerable<string> nicknames) =>
		nicknames.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

	#endregion
}
