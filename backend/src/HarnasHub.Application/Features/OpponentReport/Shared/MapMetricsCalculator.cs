using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>A roster's team-game numbers on one map; rates and share are fractions (0–1), null without games.</summary>
public record MapMetrics(
	MapName Map,
	int Games,
	int Wins,
	double? WinRate,
	double SmoothedWinRate,
	double? AvgRoundDiff,
	DateTime? LastPlayedAtUtc,
	double? RecentWinRate,
	double? EarlierWinRate,
	double Share)
{
	/// <summary>Recent minus earlier win rate; null until there are games on both sides of the split.</summary>
	public double? Trend => RecentWinRate - EarlierWinRate;
}

/// <summary>Turns a roster's team games into per-map metrics.</summary>
public static class MapMetricsCalculator
{
	#region Public Fields

	/// <summary>How many latest games on a map form the "recent" half of the trend.</summary>
	public const int TrendWindow = 5;

	#endregion

	#region Public Methods

	/// <summary>Metrics for every pool map (maps without games included, with zeros); the share is relative to all team games,
	/// off-pool maps included, so "68% of their games" means exactly that.</summary>
	public static Dictionary<MapName, MapMetrics> Calculate(IReadOnlyCollection<TeamGame> games)
	{
		var total = games.Count;

		return Enum.GetValues<MapName>().ToDictionary(map => map, map =>
		{
			var onMap = games.Where(g => g.Map == map).OrderByDescending(g => g.PlayedAtUtc).ToList();
			var wins = onMap.Count(g => g.Won);
			var recent = onMap.Take(TrendWindow).ToList();
			var earlier = onMap.Skip(TrendWindow).ToList();

			return new MapMetrics(
				map,
				onMap.Count,
				wins,
				onMap.Count == 0 ? null : (double)wins / onMap.Count,
				MapAdvantage.SmoothedWinRate(wins, onMap.Count),
				onMap.Count == 0 ? null : onMap.Average(g => g.RoundsFor - g.RoundsAgainst),
				onMap.Count == 0 ? null : onMap[0].PlayedAtUtc,
				WinRate(recent),
				WinRate(earlier),
				total == 0 ? 0 : (double)onMap.Count / total);
		});
	}

	#endregion

	#region Private Methods

	/// <summary>Plain win rate, null for an empty list.</summary>
	private static double? WinRate(List<TeamGame> games) =>
		games.Count == 0 ? null : (double)games.Count(g => g.Won) / games.Count;

	#endregion
}
