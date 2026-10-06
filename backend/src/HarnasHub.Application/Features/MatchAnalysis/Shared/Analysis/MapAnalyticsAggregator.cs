#region Usings

using HarnasHub.Application.Common.Maps;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Pure multi-match aggregation for one map: folds many stored timelines (each with its own "us") into win rates
/// per side / buy type / pistol, T-side site entries, CT retakes, opening duels per zone, trades and clutchers.
/// Matches whose team couldn't be resolved are skipped — mixing them in would credit the wrong team.</summary>
public static class MapAnalyticsAggregator
{
	#region Public Methods

	/// <summary>Aggregates <paramref name="matches"/> for <paramref name="map"/>; <paramref name="alreadySkipped"/> are
	/// matches the caller couldn't load, added to the reported skipped count.</summary>
	public static MapAnalyticsDto Aggregate(MapName map, IReadOnlyList<LoadedMatchTimeline> matches, int alreadySkipped = 0)
	{
		var usable = matches.Where(m => m.OurTeam.Count > 0).ToList();
		var rounds = new List<MatchRoundDto>();
		var openings = new Dictionary<(MapSide Side, string? Zone), (int Won, int Lost)>();
		var clutchers = new Dictionary<string, (string Name, int Attempts, int Won)>();
		int deaths = 0, traded = 0, tradeKills = 0;

		foreach (var match in usable)
		{
			var context = AnalysisContext.Create(match.Stored.Timeline, match.OurTeam);
			rounds.AddRange(MatchTimelineMapper.Map(match.Stored, match.OurTeam.ToList()).Rounds.Where(r => r.OurSide is not null && r.WeWon is not null));

			foreach (var bucket in OpeningDuelAnalyzer.Analyze(context).Buckets)
			{
				openings.TryGetValue((bucket.Side, bucket.Zone), out var current);
				openings[(bucket.Side, bucket.Zone)] = (current.Won + bucket.Won, current.Lost + bucket.Lost);
			}

			var trades = TradeAnalyzer.Analyze(context);
			deaths += trades.OurDeaths;
			traded += trades.OurTradedDeaths;
			tradeKills += trades.OurTradeKills;

			foreach (var player in ClutchAnalyzer.Analyze(context).Players)
			{
				clutchers.TryGetValue(player.SteamId64, out var current);
				clutchers[player.SteamId64] = (player.Name, current.Attempts + player.Attempts, current.Won + player.Won);
			}
		}

		var dto = new MapAnalyticsDto(
			map.ToString(),
			MapZones.HasZones(map),
			usable.Count,
			matches.Count - usable.Count + alreadySkipped,
			rounds.Count,
			SideRate(rounds, MapSide.T),
			SideRate(rounds, MapSide.CT),
			Rate(rounds.Where(r => BuyTypeClassifier.IsPistolRound(r.Number))),
			BuyTypes(rounds),
			Sites(rounds.Where(r => r.OurSide == MapSide.T && r.Bomb is not null)),
			Sites(rounds.Where(r => r.OurSide == MapSide.CT && r.Bomb is not null)),
			openings
				.Select(o => new MapOpeningZoneDto(o.Key.Side.ToString(), o.Key.Zone, o.Value.Won, o.Value.Lost))
				.OrderByDescending(o => o.Won + o.Lost).ThenBy(o => o.Zone)
				.ToList(),
			new MapTradesDto(deaths, traded, tradeKills),
			clutchers
				.Select(c => new MapClutcherDto(c.Key, c.Value.Name, c.Value.Attempts, c.Value.Won))
				.OrderByDescending(c => c.Won).ThenByDescending(c => c.Attempts).ThenBy(c => c.Name)
				.Take(5)
				.ToList(),
			[]);

		return dto with { Insights = MapInsightRules.Build(dto) };
	}

	#endregion

	#region Private Methods

	private static WinRateDto Rate(IEnumerable<MatchRoundDto> rounds)
	{
		var list = rounds.ToList();
		return new WinRateDto(list.Count(r => r.WeWon == true), list.Count);
	}

	private static WinRateDto SideRate(IEnumerable<MatchRoundDto> rounds, MapSide side) => Rate(rounds.Where(r => r.OurSide == side));

	private static List<BuyTypeWinRateDto> BuyTypes(IEnumerable<MatchRoundDto> rounds) => rounds
		.Where(r => r.OurEconomy is not null)
		.GroupBy(r => r.OurEconomy!.BuyType)
		.OrderBy(g => g.Key)
		.Select(g => new BuyTypeWinRateDto(g.Key.ToString(), g.Count(r => r.WeWon == true), g.Count()))
		.ToList();

	private static List<SiteStatDto> Sites(IEnumerable<MatchRoundDto> rounds) => rounds
		.GroupBy(r => r.Bomb!.Site?.ToString())
		.OrderBy(g => g.Key ?? "~")
		.Select(g => new SiteStatDto(g.Key, g.Count(r => r.WeWon == true), g.Count()))
		.ToList();

	#endregion
}
