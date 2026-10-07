#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure per-player totals of the opponent within one demo: rounds, kills, opening duels, AWP kills and clutches.
/// A clutch attempt is the moment a player becomes the last one alive on their side while at least one enemy still lives.</summary>
public static class OpponentPlayerFactsExtractor
{
	#region Public Methods

	/// <summary>Totals for every player who stood on the opponent's side in at least one round, most rounds first.</summary>
	public static List<OpponentPlayerFacts> Extract(DemoTimeline timeline, IReadOnlySet<long> theirTeam)
	{
		timeline = RoundParticipants.Filter(timeline);
		var totals = new Dictionary<long, int[]>();
		var names = Names(timeline);
		var killsByRound = timeline.Kills.ToLookup(k => k.RoundNumber);

		int[] Of(long id) => totals.TryGetValue(id, out var t) ? t : totals[id] = new int[7];

		foreach (var round in timeline.Rounds)
		{
			if (TimelineTeamResolver.OurSide(round, theirTeam) is not { } side)
			{
				continue;
			}

			var roster = side == MapSide.T ? round.TerroristSteamIds : round.CounterTerroristSteamIds;
			var enemies = side == MapSide.T ? round.CounterTerroristSteamIds : round.TerroristSteamIds;
			foreach (var id in roster)
			{
				Of(id)[0]++;
			}

			var rosterSet = roster.ToHashSet();
			var kills = killsByRound[round.Number].OrderBy(k => k.SecondsIntoRound).ToList();
			foreach (var kill in kills.Where(k => !k.IsTeamKill))
			{
				if (kill.Killer is { } killer && rosterSet.Contains(killer.SteamId64))
				{
					var t = Of(killer.SteamId64);
					t[1]++;
					t[2] += kill.IsOpening ? 1 : 0;
					t[4] += kill.Weapon == OpponentCtFactsExtractor.Awp ? 1 : 0;
				}
				else if (kill.IsOpening && rosterSet.Contains(kill.Victim.SteamId64))
				{
					Of(kill.Victim.SteamId64)[3]++;
				}
			}

			if (ClutchPlayer(rosterSet, enemies.ToHashSet(), kills) is { } clutcher)
			{
				var t = Of(clutcher);
				t[5]++;
				t[6] += round.WinnerSide == side ? 1 : 0;
			}
		}

		return totals
			.Select(kv => new OpponentPlayerFacts(
				kv.Key, names.GetValueOrDefault(kv.Key) ?? kv.Key.ToString(),
				kv.Value[0], kv.Value[1], kv.Value[2], kv.Value[3], kv.Value[4], kv.Value[5], kv.Value[6]))
			.OrderByDescending(p => p.Rounds)
			.ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	/// <summary>The player left alone against at least one enemy in this round, or null when it never happened.</summary>
	public static long? ClutchPlayer(IReadOnlySet<long> roster, IReadOnlySet<long> enemies, IReadOnlyList<DemoKill> killsInOrder)
	{
		var alive = roster.ToHashSet();
		var enemiesAlive = enemies.ToHashSet();

		if (alive.Count < 2)
		{
			return null;
		}

		foreach (var kill in killsInOrder)
		{
			alive.Remove(kill.Victim.SteamId64);
			enemiesAlive.Remove(kill.Victim.SteamId64);

			if (alive.Count == 1 && enemiesAlive.Count > 0 && roster.Contains(kill.Victim.SteamId64))
			{
				return alive.First();
			}

			if (alive.Count == 0)
			{
				return null;
			}
		}

		return null;
	}

	#endregion

	#region Public Helpers

	/// <summary>Player names by SteamID64 from the stats section, falling back to names seen in kills.</summary>
	public static Dictionary<long, string> Names(DemoTimeline timeline)
	{
		var names = new Dictionary<long, string>();
		foreach (var player in timeline.Stats?.Players ?? [])
		{
			names[player.SteamId64] = player.PlayerName;
		}

		foreach (var participant in timeline.Kills.SelectMany(k => new[] { k.Killer, k.Victim }))
		{
			if (participant is not null && !names.ContainsKey(participant.SteamId64))
			{
				names[participant.SteamId64] = participant.Name;
			}
		}

		return names;
	}

	#endregion
}
