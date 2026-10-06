#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Common.Demos;

/// <summary>Collects every player's display name a timeline mentions (stats first, then kills, blinds, grenades and
/// loadouts), since position tracks and round rosters carry only SteamID64s.</summary>
public static class TimelinePlayerNames
{
	#region Public Methods

	/// <summary>SteamID64 → name for everyone the timeline names; the first source that names a player wins.</summary>
	public static Dictionary<long, string> Collect(DemoTimeline timeline)
	{
		var names = new Dictionary<long, string>();

		foreach (var player in timeline.Stats?.Players ?? [])
		{
			names.TryAdd(player.SteamId64, player.PlayerName);
		}

		foreach (var participant in timeline.Kills.SelectMany(k => new[] { k.Killer, k.Victim }).OfType<DemoKillParticipant>())
		{
			names.TryAdd(participant.SteamId64, participant.Name);
		}

		foreach (var participant in timeline.Blinds.SelectMany(b => new[] { b.Attacker, b.Victim }))
		{
			names.TryAdd(participant.SteamId64, participant.Name);
		}

		foreach (var grenade in timeline.Grenades.Where(g => g.ThrowerSteamId64 is not null))
		{
			names.TryAdd(grenade.ThrowerSteamId64!.Value, grenade.ThrowerName);
		}

		foreach (var player in timeline.Economy.SelectMany(e => e.Players))
		{
			names.TryAdd(player.SteamId64, player.Name);
		}

		return names;
	}

	#endregion
}
