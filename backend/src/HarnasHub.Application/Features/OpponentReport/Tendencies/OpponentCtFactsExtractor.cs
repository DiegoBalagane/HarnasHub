#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure extraction of one opponent CT round: where each of them stood at <see cref="SetupSecond"/> (from the
/// position tracks), AWP kills, duels in the first <see cref="EarlyAggressionSeconds"/> and what they did after a plant.</summary>
public static class OpponentCtFactsExtractor
{
	#region Public Fields

	/// <summary>Second of the round (from freeze end) the default setup is read at — rotations rarely start earlier.</summary>
	public const int SetupSecond = 20;

	/// <summary>A CT kill earlier than this is an aggressive early pick rather than holding a position.</summary>
	public const float EarlyAggressionSeconds = 25f;

	/// <summary>Short weapon name of the AWP in kill events and loadouts.</summary>
	public const string Awp = "awp";

	#endregion

	#region Public Methods

	/// <summary>Extracts the CT facts of <paramref name="round"/>; <paramref name="kills"/> are the round's kills in time order.</summary>
	public static OpponentCtRoundFacts Extract(DemoTimeline timeline, DemoTimelineRound round, IReadOnlySet<long> roster, IReadOnlyList<DemoKill> kills)
	{
		var map = timeline.MapName;
		var awpers = timeline.Economy
			.Where(e => e.RoundNumber == round.Number)
			.SelectMany(e => e.Players)
			.Where(p => roster.Contains(p.SteamId64) && p.PrimaryWeapon == Awp)
			.Select(p => p.SteamId64)
			.ToHashSet();

		var setup = new List<CtSetupSpot>();
		foreach (var track in timeline.Positions.Where(t => t.RoundNumber == round.Number && roster.Contains(t.SteamId64)))
		{
			if (track.At(SetupSecond) is { RadarX: { } x, RadarY: { } y } sample)
			{
				var awp = awpers.Contains(track.SteamId64) || sample.Weapon == Awp;
				setup.Add(new CtSetupSpot(x, y, MapAreaResolver.Resolve(map, x, y), awp));
			}
		}

		var theirKills = kills.Where(k => !k.IsTeamKill && k.Killer is { } killer && roster.Contains(killer.SteamId64)).ToList();
		var awpKills = theirKills
			.Where(k => k.Weapon == Awp && k.Killer!.Position is { RadarX: not null, RadarY: not null })
			.Select(k => new AwpKillFact(
				k.Killer!.Position!.RadarX!.Value,
				k.Killer.Position.RadarY!.Value,
				MapAreaResolver.Resolve(map, k.Killer.Position.RadarX, k.Killer.Position.RadarY),
				k.SecondsIntoRound))
			.ToList();

		var earlyKills = theirKills.Count(k => k.SecondsIntoRound < EarlyAggressionSeconds);
		var earlyDeaths = kills.Count(k => !k.IsTeamKill && roster.Contains(k.Victim.SteamId64) && k.SecondsIntoRound < EarlyAggressionSeconds);

		return new OpponentCtRoundFacts(setup, awpKills, earlyKills, earlyDeaths, PostPlant(round, roster, kills));
	}

	/// <summary>Retake when any of them fought (or defused) after the plant, save when survivors didn't, all-dead when
	/// nobody was left; null when the enemy never planted.</summary>
	public static PostPlantBehaviour? PostPlant(DemoTimelineRound round, IReadOnlySet<long> roster, IReadOnlyList<DemoKill> kills)
	{
		if (round.BombPlant is not { } plant)
		{
			return null;
		}

		var deadBeforePlant = kills
			.Where(k => k.SecondsIntoRound <= plant.SecondsIntoRound && roster.Contains(k.Victim.SteamId64))
			.Select(k => k.Victim.SteamId64)
			.ToHashSet();
		if (roster.All(deadBeforePlant.Contains))
		{
			return PostPlantBehaviour.AllDead;
		}

		// Kills after the round ended (hunting saving players in post-round time) are not a retake.
		var origin = round.FreezeEndTime ?? round.StartTime;
		var endSecond = origin is { } start ? round.EndTime - start : float.MaxValue;

		var fought = round.BombDefuse is not null || kills.Any(k =>
			k.SecondsIntoRound > plant.SecondsIntoRound
			&& k.SecondsIntoRound <= endSecond
			&& !k.IsTeamKill
			&& (roster.Contains(k.Victim.SteamId64) || (k.Killer is { } killer && roster.Contains(killer.SteamId64))));

		return fought ? PostPlantBehaviour.Retake : PostPlantBehaviour.Save;
	}

	#endregion
}
