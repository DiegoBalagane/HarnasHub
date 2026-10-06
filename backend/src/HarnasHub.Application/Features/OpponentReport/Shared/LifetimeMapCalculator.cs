using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>A lineup's summed lifetime FACEIT numbers on one map (like the match room's per-map aggregate): total matches and wins of
/// the players, their average K/D and <paramref name="Share"/> — the map's fraction (0–1) of all their lifetime pool-map matches.</summary>
public record MapLifetime(MapName Map, int Players, int Matches, int Wins, double? AvgKdRatio, double Share)
{
	/// <summary>Lifetime win rate (0–1), null without matches.</summary>
	public double? WinRate => Matches == 0 ? null : (double)Wins / Matches;

	/// <summary>The lineup plays this map a lot individually: enough matches and at least
	/// <see cref="LifetimeMapCalculator.ExperiencedUniformFraction"/> of an even split across the pool.</summary>
	public bool IsExperienced =>
		Matches >= LifetimeMapCalculator.MinExperiencedMatches
		&& Share >= LifetimeMapCalculator.ExperiencedUniformFraction / Enum.GetValues<MapName>().Length;
}

/// <summary>Aggregates per-player lifetime map stats into <see cref="MapLifetime"/> per pool map — the lowest-weight signal of the
/// report (recent team games › recent solo games › lifetime).</summary>
public static class LifetimeMapCalculator
{
	#region Public Fields

	/// <summary>A map counts as "played a lot individually" from this fraction of an even split of their pool matches (0.75 × 1/7 ≈ 11%).</summary>
	public const double ExperiencedUniformFraction = 0.75;

	/// <summary>…and at least this many summed lifetime matches of the lineup on it.</summary>
	public const int MinExperiencedMatches = 30;

	/// <summary>The <c>k</c> of the lifetime shrinkage: lifetime numbers are worth this many team games, so they only nudge.</summary>
	public const double PriorGames = 2;

	#endregion

	#region Public Methods

	/// <summary>Per-map aggregate over the given players' lifetime stats (one list per player); all zeros without data.</summary>
	public static Dictionary<MapName, MapLifetime> Calculate(IEnumerable<IReadOnlyList<FaceitLifetimeMapStats>> players)
	{
		var perPlayer = players
			.Select(stats => stats
				.Select(s => (Map: TeamMatchDetector.ParseMap(s.MapName), Stat: s))
				.Where(x => x.Map.HasValue && x.Stat.Matches > 0)
				.GroupBy(x => x.Map!.Value)
				.ToDictionary(
					g => g.Key,
					g => (Matches: g.Sum(x => x.Stat.Matches), Wins: g.Sum(x => x.Stat.Wins), Kd: g.Select(x => x.Stat.KdRatio).FirstOrDefault(k => k.HasValue))))
			.ToList();
		var total = perPlayer.Sum(p => p.Values.Sum(v => v.Matches));

		return Enum.GetValues<MapName>().ToDictionary(map => map, map =>
		{
			var onMap = perPlayer.Where(p => p.ContainsKey(map)).Select(p => p[map]).ToList();
			var kds = onMap.Where(x => x.Kd.HasValue).Select(x => x.Kd!.Value).ToList();
			var matches = onMap.Sum(x => x.Matches);

			return new MapLifetime(
				map,
				onMap.Count,
				matches,
				onMap.Sum(x => x.Wins),
				kds.Count == 0 ? null : kds.Average(),
				total == 0 ? 0 : (double)matches / total);
		});
	}

	/// <summary>Lifetime preference on the team scale (share + smoothed win rate); 0 without matches.</summary>
	public static double Preference(MapLifetime lifetime) =>
		lifetime.Matches == 0 ? 0 : lifetime.Share + MapAdvantage.SmoothedWinRate(lifetime.Wins, lifetime.Matches);

	/// <summary><c>(1 − w)·preference + w·lifetimePreference</c> with <c>w = k / (k + evidence)</c>, <c>k = </c><see cref="PriorGames"/>;
	/// unchanged without lifetime matches.</summary>
	public static double Blend(double preference, double evidence, MapLifetime? lifetime)
	{
		if (lifetime is not { Matches: > 0 })
		{
			return preference;
		}

		var weight = PriorGames / (PriorGames + Math.Max(0, evidence));
		return (1 - weight) * preference + weight * Preference(lifetime);
	}

	/// <summary>The DTO form (percentages); null when nobody has lifetime matches on the map.</summary>
	public static MapLifetimeDto? ToDto(MapLifetime? lifetime) =>
		lifetime is not { Matches: > 0 }
			? null
			: new MapLifetimeDto(
				lifetime.Players,
				lifetime.Matches,
				lifetime.WinRate is { } wr ? Math.Round(wr * 100, 1) : null,
				lifetime.AvgKdRatio is { } kd ? Math.Round(kd, 2) : null,
				Math.Round(lifetime.Share * 100, 1),
				lifetime.IsExperienced);

	#endregion
}
