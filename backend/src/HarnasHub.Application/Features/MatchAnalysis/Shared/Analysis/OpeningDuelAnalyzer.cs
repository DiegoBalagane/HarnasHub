#region Usings

using HarnasHub.Application.Common.Maps;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Pure opening-duel breakdown: every round's first enemy kill that involved one of us, with the zone it happened
/// in (the victim's, falling back to the killer's), rolled up per side+zone and per player.</summary>
public static class OpeningDuelAnalyzer
{
	#region Public Methods

	/// <summary>Collects our opening duels and the rollups.</summary>
	public static OpeningSummaryDto Analyze(AnalysisContext context)
	{
		var map = context.Timeline.MapName;
		var duels = new List<OpeningDuelDto>();
		var players = new Dictionary<long, (int Won, int Lost)>();

		foreach (var kill in context.Timeline.Kills.Where(k => k.IsOpening && k.Killer is not null && !k.IsTeamKill))
		{
			var killer = kill.Killer!;
			var killerOurs = context.IsOurs(killer.SteamId64);
			if (killerOurs == context.IsOurs(kill.Victim.SteamId64))
			{
				continue;
			}

			var zone = MapZones.Find(map, kill.Victim.Position?.RadarX, kill.Victim.Position?.RadarY)?.Name
				?? MapZones.Find(map, killer.Position?.RadarX, killer.Position?.RadarY)?.Name;

			duels.Add(new OpeningDuelDto(
				kill.RoundNumber,
				killerOurs ? killer.Side : kill.Victim.Side,
				killerOurs,
				zone,
				killer.Name,
				kill.Victim.Name,
				killer.Position?.RadarX,
				killer.Position?.RadarY,
				kill.Victim.Position?.RadarX,
				kill.Victim.Position?.RadarY));

			var ourPlayer = killerOurs ? killer.SteamId64 : kill.Victim.SteamId64;
			players.TryGetValue(ourPlayer, out var current);
			players[ourPlayer] = killerOurs ? (current.Won + 1, current.Lost) : (current.Won, current.Lost + 1);
		}

		var buckets = duels
			.Where(d => d.OurSide is not null)
			.GroupBy(d => (Side: d.OurSide!.Value, d.Zone))
			.Select(g => new OpeningBucketDto(g.Key.Side, g.Key.Zone, g.Count(d => d.WonByUs), g.Count(d => !d.WonByUs)))
			.OrderByDescending(b => b.Won + b.Lost).ThenBy(b => b.Zone)
			.ToList();

		var rows = players
			.Select(p => new OpeningPlayerDto(p.Key.ToString(), context.Name(p.Key), p.Value.Won, p.Value.Lost))
			.OrderByDescending(p => p.Won + p.Lost).ThenBy(p => p.Name)
			.ToList();

		return new OpeningSummaryDto(duels.OrderBy(d => d.RoundNumber).ToList(), buckets, rows);
	}

	#endregion
}
