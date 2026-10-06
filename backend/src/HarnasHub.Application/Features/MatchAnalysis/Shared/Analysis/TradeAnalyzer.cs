#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Pure trade detection: a death to an enemy is "traded" when a teammate of the victim kills the killer within
/// <see cref="WindowSeconds"/> in the same round; that teammate is credited with a trade kill. World deaths and team
/// kills never count as trades or as tradeable deaths.</summary>
public static class TradeAnalyzer
{
	#region Public Fields

	/// <summary>Maximum seconds between a death and its avenging kill.</summary>
	public const float WindowSeconds = 5f;

	#endregion

	#region Public Methods

	/// <summary>Computes both teams' trade totals and our players' rows (most trade kills first).</summary>
	public static TradeSummaryDto Analyze(AnalysisContext context)
	{
		var players = new Dictionary<long, (int Deaths, int Traded, int TradeKills)>();
		int ourDeaths = 0, ourTraded = 0, ourTradeKills = 0, theirDeaths = 0, theirTraded = 0;

		foreach (var round in context.Timeline.Kills.GroupBy(k => k.RoundNumber))
		{
			var enemyKills = round
				.Where(k => k.Killer is not null && !k.IsTeamKill)
				.OrderBy(k => k.SecondsIntoRound)
				.ToList();

			foreach (var death in enemyKills)
			{
				var avenger = FindAvenger(enemyKills, death);
				var traded = avenger is null ? 0 : 1;

				if (context.IsOurs(death.Victim.SteamId64))
				{
					ourDeaths++;
					ourTraded += traded;
					Bump(players, death.Victim.SteamId64, 1, traded, 0);
				}
				else
				{
					theirDeaths++;
					theirTraded += traded;
				}

				if (avenger?.Killer is { } trader && context.IsOurs(trader.SteamId64))
				{
					ourTradeKills++;
					Bump(players, trader.SteamId64, 0, 0, 1);
				}
			}
		}

		var rows = players
			.Select(p => new TradePlayerDto(p.Key.ToString(), context.Name(p.Key), p.Value.Deaths, p.Value.Traded, p.Value.TradeKills))
			.OrderByDescending(p => p.TradeKills).ThenByDescending(p => p.TradedDeaths).ThenBy(p => p.Name)
			.ToList();

		return new TradeSummaryDto(ourDeaths, ourTraded, ourTradeKills, theirDeaths, theirTraded, rows);
	}

	#endregion

	#region Private Methods

	private static DemoKill? FindAvenger(List<DemoKill> enemyKills, DemoKill death) =>
		enemyKills.FirstOrDefault(k =>
			!ReferenceEquals(k, death)
			&& k.SecondsIntoRound >= death.SecondsIntoRound
			&& k.SecondsIntoRound - death.SecondsIntoRound <= WindowSeconds
			&& k.Victim.SteamId64 == death.Killer!.SteamId64
			&& k.Killer!.Side == death.Victim.Side);

	private static void Bump(Dictionary<long, (int Deaths, int Traded, int TradeKills)> map, long id, int deaths, int traded, int tradeKills)
	{
		map.TryGetValue(id, out var current);
		map[id] = (current.Deaths + deaths, current.Traded + traded, current.TradeKills + tradeKills);
	}

	#endregion
}
