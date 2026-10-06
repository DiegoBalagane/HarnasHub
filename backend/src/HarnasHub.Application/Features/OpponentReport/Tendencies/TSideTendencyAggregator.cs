#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>A round's facts with the index of the demo it came from (round numbers repeat across demos).</summary>
public record IndexedRound(int DemoIndex, OpponentRoundFacts Round);

/// <summary>Pure aggregation of the opponent's T rounds (and pistol rounds of both sides) across demos of one map.</summary>
public static class TSideTendencyAggregator
{
	#region Public Fields

	/// <summary>An execute before this second is fast.</summary>
	public const float FastBefore = 35f;

	/// <summary>An execute after this second is late.</summary>
	public const float LateAfter = 75f;

	/// <summary>Most grenade clusters reported per map.</summary>
	public const int MaxClusters = 12;

	#endregion

	#region Public Methods

	/// <summary>Aggregates the T side; <paramref name="rounds"/> holds every round of every demo (both sides).</summary>
	public static TSideTendenciesDto Aggregate(MapName map, IReadOnlyList<IndexedRound> rounds)
	{
		var tRounds = rounds.Where(r => r.Round.T is not null).ToList();
		var facts = tRounds.Select(r => r.Round.T!).ToList();

		var targets = Shares(facts.Where(f => f.Target is not null).Select(f => f.Target!.Value.ToString()).ToList());

		var execs = facts.Where(f => f.ExecSecond is not null).Select(f => f.ExecSecond!.Value).ToList();
		var timing = Shares(execs.Select(Timing).ToList());

		var contacts = facts.Where(f => f is { FirstContact: not null, FirstContactArea: not null }).ToList();
		var entries = contacts
			.GroupBy(f => f.FirstContactArea!.Value)
			.Select(g => new EntryArrowDto(
				g.Key.ToString(),
				g.Average(f => f.FirstContact!.X),
				g.Average(f => f.FirstContact!.Y),
				g.Count(),
				TendencyConfidence.Percent(g.Count(), contacts.Count)))
			.OrderByDescending(e => e.Count)
			.ToList();

		var clusters = GrenadeClusterer
			.Cluster(tRounds.SelectMany(r => r.Round.T!.Grenades.Select(g => new ClusterInput(g.Type, g.X, g.Y, r.DemoIndex, r.Round.Number))))
			.Take(MaxClusters)
			.Select(c => new GrenadeClusterDto(
				c.Type.ToString(), c.X, c.Y, c.Throws, c.Rounds,
				TendencyConfidence.Percent(c.Rounds, tRounds.Count),
				MapAreaResolver.Resolve(map, c.X, c.Y)?.ToString()))
			.ToList();

		return new TSideTendenciesDto(
			tRounds.Count,
			TendencyConfidence.For(tRounds.Count),
			targets,
			timing,
			execs.Count == 0 ? null : Math.Round(execs.Average(), 1),
			entries,
			clusters,
			Pistol(rounds));
	}

	/// <summary>"Fast", "Mid" or "Late" for an execute second.</summary>
	public static string Timing(float second) => second < FastBefore ? "Fast" : second > LateAfter ? "Late" : "Mid";

	/// <summary>Distribution of labels, most frequent first.</summary>
	public static List<TendencyShareDto> Shares(IReadOnlyList<string> labels) =>
		labels
			.GroupBy(l => l)
			.Select(g => new TendencyShareDto(g.Key, g.Count(), TendencyConfidence.Percent(g.Count(), labels.Count)))
			.OrderByDescending(s => s.Count)
			.ThenBy(s => s.Label, StringComparer.Ordinal)
			.ToList();

	#endregion

	#region Private Methods

	private static PistolTendencyDto Pistol(IReadOnlyList<IndexedRound> rounds)
	{
		var pistols = rounds.Where(r => BuyTypeClassifier.IsPistolRound(r.Round.Number)).ToList();
		var tPistolTargets = Shares(pistols
			.Where(r => r.Round.T?.Target is not null)
			.Select(r => r.Round.T!.Target!.Value.ToString())
			.ToList());

		var lost = pistols.Where(r => r.Round.Won == false).ToList();
		var byKey = rounds.ToDictionary(r => (r.DemoIndex, r.Round.Number));
		var afterBuys = lost
			.Select(r => byKey.GetValueOrDefault((r.DemoIndex, r.Round.Number + 1))?.Round.Buy)
			.Where(b => b is not null)
			.Select(b => b!)
			.ToList();

		return new PistolTendencyDto(pistols.Count, pistols.Count(r => r.Round.Won == true), tPistolTargets, lost.Count, Shares(afterBuys));
	}

	#endregion
}
