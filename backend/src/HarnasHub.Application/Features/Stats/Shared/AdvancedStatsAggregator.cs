#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>One analysed match ready to be folded: its result row, the analysis context ("us" already resolved) and the map label.</summary>
public record AdvancedMatchInput(MatchResult Result, AnalysisContext Context, string Map);

/// <summary>Pure fold of the per-match analyzers (trades, clutches, opening duels, flashes) and stored stat lines into
/// <see cref="AdvancedStatsDto"/>. Only the given users (roster members shown in stats, matched by SteamID64) are reported.</summary>
public static class AdvancedStatsAggregator
{
	#region Public Methods

	/// <summary>Folds <paramref name="matches"/>; <paramref name="stats"/> are the stored stat lines of those matches.</summary>
	public static AdvancedStatsDto Aggregate(
		IReadOnlyList<AdvancedMatchInput> matches,
		IReadOnlyList<User> users,
		IReadOnlyList<PlayerMatchStat> stats,
		int skipped = 0)
	{
		var bySteam = new Dictionary<long, User>();
		foreach (var user in users)
		{
			if (long.TryParse(user.SteamId64, out var steamId))
			{
				bySteam[steamId] = user;
			}
		}

		var acc = new Dictionary<Guid, Accumulator>();
		var statLookup = stats.Where(s => s.UserId != null).ToLookup(s => (s.MatchResultId, UserId: s.UserId!.Value));
		var cells = new Dictionary<(Guid User, string Map), List<PlayerMatchStat>>();
		var form = new List<AdvancedFormPointDto>();

		foreach (var match in matches)
		{
			var context = match.Context;
			var trades = TradeAnalyzer.Analyze(context).Players.ToDictionary(p => p.SteamId64);
			var clutches = ClutchAnalyzer.Analyze(context).Players.ToDictionary(p => p.SteamId64);
			var flashes = FlashAnalyzer.Analyze(context).Players.ToDictionary(p => p.SteamId64);
			var openings = OpeningBySide(context);

			foreach (var (steamId, user) in bySteam)
			{
				var rounds = context.Timeline.Rounds.Count(r => r.TerroristSteamIds.Contains(steamId) || r.CounterTerroristSteamIds.Contains(steamId));
				if (!context.IsOurs(steamId) || rounds == 0)
				{
					continue;
				}

				var key = steamId.ToString();
				if (!acc.TryGetValue(user.Id, out var player))
				{
					acc[user.Id] = player = new Accumulator(user);
				}

				player.Matches++;
				player.Rounds += rounds;
				player.Name = context.Name(steamId);

				if (openings.TryGetValue(steamId, out var open))
				{
					player.OpeningWonT += open.WonT;
					player.OpeningLostT += open.LostT;
					player.OpeningWonCt += open.WonCt;
					player.OpeningLostCt += open.LostCt;
				}

				if (trades.TryGetValue(key, out var trade))
				{
					player.TradeKills += trade.TradeKills;
					player.Deaths += trade.Deaths;
					player.TradedDeaths += trade.TradedDeaths;
				}

				if (clutches.TryGetValue(key, out var clutch))
				{
					player.ClutchAttempts += clutch.Attempts;
					player.ClutchesWon += clutch.Won;
					player.BestClutchWon = Math.Max(player.BestClutchWon, clutch.BestWonVersus);
				}

				if (flashes.TryGetValue(key, out var flash))
				{
					player.EnemiesFlashed += flash.EnemiesFlashed;
					player.BlindSecondsSum += flash.AvgEnemyBlindSeconds * flash.EnemiesFlashed;
					player.TeamFlashes += flash.TeamFlashes;
				}

				var line = statLookup[(match.Result.Id, user.Id)].FirstOrDefault();
				if (line is null)
				{
					continue;
				}

				player.Lines.Add(line);
				if (line.UtilityDamage is { } utility)
				{
					player.UtilityDamage += utility;
					player.UtilityMatches++;
				}

				var cellKey = (user.Id, match.Map);
				if (!cells.TryGetValue(cellKey, out var cellLines))
				{
					cells[cellKey] = cellLines = [];
				}

				cellLines.Add(line);
				form.Add(new AdvancedFormPointDto(user.Id, match.Result.Id, match.Result.PlayedAtUtc, match.Result.Opponent, match.Map, line.Rating, line.Adr));
			}
		}

		var players = acc.Values.Select(a => a.ToDto()).OrderByDescending(p => p.AvgRating ?? 0).ThenBy(p => p.Name).ToList();
		var mapCells = cells
			.Select(c => new AdvancedMapCellDto(c.Key.User, c.Key.Map, c.Value.Count, Math.Round(c.Value.Average(l => l.Rating), 2), Math.Round(c.Value.Average(l => l.Adr), 1)))
			.OrderBy(c => c.Map).ThenBy(c => c.UserId)
			.ToList();

		return new AdvancedStatsDto(matches.Count, skipped, players, mapCells, form.OrderBy(f => f.PlayedAtUtc).ToList());
	}

	#endregion

	#region Private Methods

	/// <summary>Our players' opening duels won/lost on each side (the analyzer rollup has no side split).</summary>
	private static Dictionary<long, (int WonT, int LostT, int WonCt, int LostCt)> OpeningBySide(AnalysisContext context)
	{
		var result = new Dictionary<long, (int WonT, int LostT, int WonCt, int LostCt)>();

		foreach (var kill in context.Timeline.Kills.Where(k => k.IsOpening && k.Killer is not null && !k.IsTeamKill))
		{
			var killer = kill.Killer!;
			var killerOurs = context.IsOurs(killer.SteamId64);
			if (killerOurs == context.IsOurs(kill.Victim.SteamId64))
			{
				continue;
			}

			var ours = killerOurs ? killer : kill.Victim;
			result.TryGetValue(ours.SteamId64, out var current);
			var onT = ours.Side == MapSide.T;
			result[ours.SteamId64] = (killerOurs, onT) switch
			{
				(true, true) => (current.WonT + 1, current.LostT, current.WonCt, current.LostCt),
				(false, true) => (current.WonT, current.LostT + 1, current.WonCt, current.LostCt),
				(true, false) => (current.WonT, current.LostT, current.WonCt + 1, current.LostCt),
				_ => (current.WonT, current.LostT, current.WonCt, current.LostCt + 1)
			};
		}

		return result;
	}

	#endregion

	#region Nested Types

	private sealed class Accumulator(User user)
	{
		public string Name { get; set; } = user.InGameNickname ?? user.DisplayName;
		public int Matches, Rounds, OpeningWonT, OpeningLostT, OpeningWonCt, OpeningLostCt, TradeKills, Deaths, TradedDeaths;
		public int ClutchAttempts, ClutchesWon, BestClutchWon, EnemiesFlashed, TeamFlashes, UtilityDamage, UtilityMatches;
		public double BlindSecondsSum;
		public List<PlayerMatchStat> Lines { get; } = [];

		public AdvancedPlayerDto ToDto()
		{
			var kasts = Lines.Where(l => l.KastPercentage.HasValue).Select(l => l.KastPercentage!.Value).ToList();

			return new AdvancedPlayerDto(
				user.Id, Name, Matches, Rounds,
				OpeningWonT, OpeningLostT, OpeningWonCt, OpeningLostCt,
				TradeKills, Deaths, TradedDeaths,
				ClutchAttempts, ClutchesWon, BestClutchWon,
				EnemiesFlashed, EnemiesFlashed == 0 ? 0 : Math.Round(BlindSecondsSum / EnemiesFlashed, 2), TeamFlashes,
				UtilityMatches == 0 ? null : Math.Round((double)UtilityDamage / UtilityMatches, 1),
				kasts.Count == 0 ? null : Math.Round(kasts.Average(), 1),
				Lines.Count == 0 ? null : Math.Round(Lines.Average(l => l.Rating), 2),
				Lines.Count == 0 ? null : Math.Round(Lines.Average(l => l.Adr), 1));
		}
	}

	#endregion
}
