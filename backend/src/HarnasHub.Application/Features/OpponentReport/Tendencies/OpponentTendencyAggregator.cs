#region Usings

using HarnasHub.Application.Common.Maps;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure aggregation of every analysed opponent demo on one map into <see cref="MapTendenciesDto"/>: T and CT side
/// via their aggregators, players across demos (by SteamID64) with roles, then <see cref="AntiStratRules"/>.</summary>
public static class OpponentTendencyAggregator
{
	#region Public Fields

	/// <summary>Players with fewer rounds than this (a one-off stand-in) are left out of the player list.</summary>
	public const int MinPlayerRounds = 5;

	/// <summary>Most players listed.</summary>
	public const int MaxPlayers = 7;

	/// <summary>Share of the team's AWP kills that makes a player "the AWPer".</summary>
	public const double AwperShare = 40;

	/// <summary>Opening-duel participation (percent of rounds) that makes the top such player "the entry".</summary>
	public const double EntryRate = 20;

	#endregion

	#region Public Methods

	/// <summary>Tendencies of the map from the facts of each demo played on it.</summary>
	public static MapTendenciesDto Aggregate(MapName map, IReadOnlyList<OpponentDemoFacts> demos)
	{
		var rounds = demos
			.SelectMany((demo, index) => demo.Rounds.Select(round => new IndexedRound(index, round)))
			.ToList();

		var spawn = MapAreaResolver.TSpawnCentre(map);
		var tendencies = new MapTendenciesDto(
			map.ToString(),
			demos.Count,
			rounds.Count,
			MapZones.HasZones(map),
			spawn?.X,
			spawn?.Y,
			TSideTendencyAggregator.Aggregate(map, rounds),
			CtSideTendencyAggregator.Aggregate(rounds),
			Players(demos),
			[]);

		return tendencies with { Suggestions = AntiStratRules.Evaluate(tendencies) };
	}

	/// <summary>Players summed across demos, most rounds first, each with at most one role.</summary>
	public static List<PlayerTendencyDto> Players(IReadOnlyList<OpponentDemoFacts> demos)
	{
		var players = demos
			.SelectMany(d => d.Players)
			.GroupBy(p => p.SteamId64)
			.Select(g => new OpponentPlayerFacts(
				g.Key,
				g.Last().Name,
				g.Sum(p => p.Rounds),
				g.Sum(p => p.Kills),
				g.Sum(p => p.OpeningKills),
				g.Sum(p => p.OpeningDeaths),
				g.Sum(p => p.AwpKills),
				g.Sum(p => p.ClutchAttempts),
				g.Sum(p => p.ClutchWins)))
			.Where(p => p.Rounds >= MinPlayerRounds)
			.OrderByDescending(p => p.Rounds)
			.Take(MaxPlayers)
			.ToList();

		var teamAwpKills = players.Sum(p => p.AwpKills);
		var awper = players
			.Where(p => p.AwpKills >= 3 && TendencyConfidence.Percent(p.AwpKills, teamAwpKills) >= AwperShare)
			.MaxBy(p => p.AwpKills);
		var entry = players
			.Where(p => p != awper && TendencyConfidence.Percent(p.OpeningKills + p.OpeningDeaths, p.Rounds) >= EntryRate)
			.MaxBy(p => (double)(p.OpeningKills + p.OpeningDeaths) / p.Rounds);
		var clutcher = players
			.Where(p => p != awper && p != entry && p.ClutchWins >= 2)
			.MaxBy(p => p.ClutchWins);

		return players
			.Select(p => new PlayerTendencyDto(
				p.SteamId64.ToString(),
				p.Name,
				p.Rounds,
				p.Kills,
				TendencyConfidence.Percent(p.OpeningKills + p.OpeningDeaths, p.Rounds),
				p.OpeningKills + p.OpeningDeaths == 0 ? null : TendencyConfidence.Percent(p.OpeningKills, p.OpeningKills + p.OpeningDeaths),
				p.AwpKills,
				TendencyConfidence.Percent(p.AwpKills, teamAwpKills),
				p.ClutchAttempts,
				p.ClutchWins,
				p == awper ? "AWP" : p == entry ? "Entry" : p == clutcher ? "Clutch" : null))
			.ToList();
	}

	#endregion
}
