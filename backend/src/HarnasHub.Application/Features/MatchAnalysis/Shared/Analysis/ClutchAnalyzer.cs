#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Pure 1vX detection: replays each round's deaths against its starting rosters and records, per side, the first
/// moment exactly one player is left while at least one enemy still lives. The round winner decides won/lost.</summary>
public static class ClutchAnalyzer
{
	#region Public Methods

	/// <summary>Finds every clutch in the match and rolls them up for both teams and our players.</summary>
	public static ClutchSummaryDto Analyze(AnalysisContext context)
	{
		var clutches = new List<ClutchDto>();

		foreach (var round in context.Timeline.Rounds.OrderBy(r => r.Number))
		{
			var deaths = context.Timeline.Kills.Where(k => k.RoundNumber == round.Number).OrderBy(k => k.SecondsIntoRound).ToList();
			clutches.AddRange(InRound(context, round, deaths));
		}

		var ours = clutches.Where(c => c.IsOurs).ToList();
		var players = ours
			.GroupBy(c => c.SteamId64)
			.Select(g => new ClutchPlayerDto(
				g.Key,
				g.First().Name,
				g.Count(),
				g.Count(c => c.Won),
				g.Where(c => c.Won).Select(c => c.Versus).DefaultIfEmpty(0).Max()))
			.OrderByDescending(p => p.Won).ThenByDescending(p => p.Attempts).ThenBy(p => p.Name)
			.ToList();

		var theirs = clutches.Where(c => !c.IsOurs).ToList();
		return new ClutchSummaryDto(ours.Count, ours.Count(c => c.Won), theirs.Count, theirs.Count(c => c.Won), players, clutches);
	}

	#endregion

	#region Private Methods

	private static List<ClutchDto> InRound(AnalysisContext context, DemoTimelineRound round, List<DemoKill> deaths)
	{
		var result = new List<ClutchDto>();
		var alive = new Dictionary<MapSide, HashSet<long>>
		{
			[MapSide.T] = round.TerroristSteamIds.ToHashSet(),
			[MapSide.CT] = round.CounterTerroristSteamIds.ToHashSet()
		};
		var recorded = new HashSet<MapSide>();

		foreach (var death in deaths)
		{
			alive[MapSide.T].Remove(death.Victim.SteamId64);
			alive[MapSide.CT].Remove(death.Victim.SteamId64);

			foreach (var side in new[] { MapSide.T, MapSide.CT })
			{
				var members = alive[side];
				var enemies = alive[side == MapSide.T ? MapSide.CT : MapSide.T];
				if (members.Count != 1 || enemies.Count < 1 || !recorded.Add(side))
				{
					continue;
				}

				var player = members.First();
				result.Add(new ClutchDto(
					round.Number,
					player.ToString(),
					context.Name(player),
					enemies.Count,
					round.WinnerSide == side,
					context.IsOurs(player)));
			}
		}

		return result;
	}

	#endregion
}
