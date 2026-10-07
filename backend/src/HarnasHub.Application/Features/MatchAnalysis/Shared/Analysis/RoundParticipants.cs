#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Pure read-side filter that drops non-playing roster members (coaches, spectators, disconnected accounts) from
/// every round's T/CT rosters, so they never count as "alive" teammates. Needed for timelines stored before the parser
/// stopped listing them. Evidence of playing: a position track in that round, or any
/// appearance in the match's kills, blinds or economy snapshots (a coach has none of them).</summary>
public static class RoundParticipants
{
	#region Public Methods

	/// <summary>Treats the manually excluded players as if they never played: drops them from rosters, tracks, loadouts, stats,
	/// grenades, blinds and kills (a kill with an excluded killer or victim goes away entirely, an excluded assister is cleared).</summary>
	public static DemoTimeline Exclude(DemoTimeline timeline, IReadOnlyCollection<long> excluded)
	{
		if (excluded.Count == 0)
		{
			return timeline;
		}

		var hidden = excluded.ToHashSet();
		DemoKillParticipant? Keep(DemoKillParticipant? p) => p is not null && hidden.Contains(p.SteamId64) ? null : p;

		return timeline with
		{
			Stats = timeline.Stats is null ? null : timeline.Stats with { Players = timeline.Stats.Players.Where(p => !hidden.Contains(p.SteamId64)).ToList() },
			Rounds = timeline.Rounds.Select(r => r with
			{
				TerroristSteamIds = r.TerroristSteamIds.Where(id => !hidden.Contains(id)).ToList(),
				CounterTerroristSteamIds = r.CounterTerroristSteamIds.Where(id => !hidden.Contains(id)).ToList()
			}).ToList(),
			Positions = timeline.Positions.Where(t => !hidden.Contains(t.SteamId64)).ToList(),
			Economy = timeline.Economy.Select(e => e with { Players = e.Players.Where(p => !hidden.Contains(p.SteamId64)).ToList() }).ToList(),
			Grenades = timeline.Grenades.Where(g => g.ThrowerSteamId64 is not { } id || !hidden.Contains(id)).ToList(),
			Blinds = timeline.Blinds.Where(b => !hidden.Contains(b.Attacker.SteamId64) && !hidden.Contains(b.Victim.SteamId64)).ToList(),
			Kills = timeline.Kills
				.Where(k => !hidden.Contains(k.Victim.SteamId64) && (k.Killer is null || !hidden.Contains(k.Killer.SteamId64)))
				.Select(k => k with { Assister = Keep(k.Assister) })
				.ToList()
		};
	}


	/// <summary>A copy of <paramref name="timeline"/> whose round rosters hold only players who took part (unchanged when nothing is dropped).</summary>
	public static DemoTimeline Filter(DemoTimeline timeline)
	{
		var tracksByRound = timeline.Positions.ToLookup(t => t.RoundNumber, t => t.SteamId64);
		var matchWide = MatchWide(timeline);
		var changed = false;

		var rounds = timeline.Rounds.Select(round =>
		{
			var evidence = tracksByRound[round.Number].Concat(matchWide).ToHashSet();

			var t = round.TerroristSteamIds.Where(id => IsPlaying(id, evidence)).ToList();
			var ct = round.CounterTerroristSteamIds.Where(id => IsPlaying(id, evidence)).ToList();
			if (t.Count == round.TerroristSteamIds.Count && ct.Count == round.CounterTerroristSteamIds.Count)
			{
				return round;
			}

			changed = true;
			return round with { TerroristSteamIds = t, CounterTerroristSteamIds = ct };
		}).ToList();

		return changed ? timeline with { Rounds = rounds } : timeline;
	}

	#endregion

	#region Private Methods

	// Bots share SteamID 0 and are never filtered; an empty evidence set (nothing recorded at all) filters nothing.
	private static bool IsPlaying(long steamId64, HashSet<long> evidence) => steamId64 == 0 || evidence.Count == 0 || evidence.Contains(steamId64);

	private static HashSet<long> MatchWide(DemoTimeline timeline)
	{
		var ids = new HashSet<long>();
		foreach (var kill in timeline.Kills)
		{
			ids.Add(kill.Victim.SteamId64);
			if (kill.Killer is { } killer)
			{
				ids.Add(killer.SteamId64);
			}

			if (kill.Assister is { } assister)
			{
				ids.Add(assister.SteamId64);
			}
		}

		foreach (var blind in timeline.Blinds)
		{
			ids.Add(blind.Attacker.SteamId64);
			ids.Add(blind.Victim.SteamId64);
		}

		foreach (var player in timeline.Economy.SelectMany(e => e.Players))
		{
			ids.Add(player.SteamId64);
		}

		return ids;
	}

	#endregion
}
