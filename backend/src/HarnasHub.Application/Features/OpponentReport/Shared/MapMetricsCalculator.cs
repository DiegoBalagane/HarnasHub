using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>A roster's team-game numbers on one map; rates and share are fractions (0–1), null without games. Counts and
/// <paramref name="WinRate"/>/<paramref name="Share"/> are raw (shown on screen); <paramref name="SmoothedWinRate"/> and
/// <see cref="WeightedShare"/> are recency-weighted (<see cref="RecencyWeight"/>) and drive the decisions.</summary>
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

	/// <summary>Sum of the games' recency weights; null means unweighted (equal to <see cref="Games"/>).</summary>
	public double? WeightedGames { get; init; }

	/// <summary>Sum of the won games' recency weights; null means unweighted (equal to <see cref="Wins"/>).</summary>
	public double? WeightedWins { get; init; }

	/// <summary>Recency-weighted share of their team games; null means unweighted (equal to <see cref="Share"/>).</summary>
	public double? WeightedShare { get; init; }

	/// <summary>The share the decisions use: weighted when known, otherwise raw.</summary>
	public double DecisionShare => WeightedShare ?? Share;
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
	/// off-pool maps included, so "68% of their games" means exactly that. With <paramref name="nowUtc"/> the smoothed win rate and
	/// the decision share are recency-weighted; without it every game weighs 1.</summary>
	public static Dictionary<MapName, MapMetrics> Calculate(IReadOnlyCollection<TeamGame> games, DateTime? nowUtc = null)
	{
		var total = games.Count;
		var weightedTotal = games.Sum(g => RecencyWeight.Of(g.PlayedAtUtc, nowUtc));

		return Enum.GetValues<MapName>().ToDictionary(map => map, map =>
		{
			var onMap = games.Where(g => g.Map == map).OrderByDescending(g => g.PlayedAtUtc).ToList();
			var wins = onMap.Count(g => g.Won);
			var recent = onMap.Take(TrendWindow).ToList();
			var earlier = onMap.Skip(TrendWindow).ToList();
			var weightedGames = onMap.Sum(g => RecencyWeight.Of(g.PlayedAtUtc, nowUtc));
			var weightedWins = onMap.Where(g => g.Won).Sum(g => RecencyWeight.Of(g.PlayedAtUtc, nowUtc));

			return new MapMetrics(
				map,
				onMap.Count,
				wins,
				onMap.Count == 0 ? null : (double)wins / onMap.Count,
				MapAdvantage.SmoothedWinRate(weightedWins, weightedGames),
				onMap.Count == 0 ? null : onMap.Average(g => g.RoundsFor - g.RoundsAgainst),
				onMap.Count == 0 ? null : onMap[0].PlayedAtUtc,
				WinRate(recent),
				WinRate(earlier),
				total == 0 ? 0 : (double)onMap.Count / total)
			{
				WeightedGames = weightedGames,
				WeightedWins = weightedWins,
				WeightedShare = weightedTotal <= 0 ? 0 : weightedGames / weightedTotal
			};
		});
	}

	#endregion

	#region Private Methods

	/// <summary>Plain win rate, null for an empty list.</summary>
	private static double? WinRate(List<TeamGame> games) =>
		games.Count == 0 ? null : (double)games.Count(g => g.Won) / games.Count;

	#endregion
}
