#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport;

/// <summary>Hand-made Mirage timelines and facts for the opponent demo tests. "Them" (11–15) start on T as team A,
/// the other team (21–25) on CT as team B; points are radar fractions inside the Mirage zone polygons.</summary>
public static class OpponentTimelineFactory
{
	#region Public Fields

	/// <summary>The opponent's SteamIDs.</summary>
	public static readonly long[] Them = [11, 12, 13, 14, 15];

	/// <summary>The other team's SteamIDs.</summary>
	public static readonly long[] Others = [21, 22, 23, 24, 25];

	/// <summary>A point on bombsite A.</summary>
	public static readonly (float X, float Y) SiteA = (0.53f, 0.82f);

	/// <summary>A point on bombsite B.</summary>
	public static readonly (float X, float Y) SiteB = (0.15f, 0.22f);

	/// <summary>A point in mid.</summary>
	public static readonly (float X, float Y) Mid = (0.57f, 0.44f);

	/// <summary>A point in T spawn.</summary>
	public static readonly (float X, float Y) TSpawn = (0.94f, 0.32f);

	#endregion

	#region Public Methods

	/// <summary>A round in which "them" stand on <paramref name="theirSide"/>.</summary>
	public static DemoTimelineRound Round(int number, MapSide theirSide, MapSide? winner, DemoBombEvent? plant = null) => new(
		number, 0f, 15f, 130f, winner, DemoRoundEndReason.Elimination,
		theirSide == MapSide.T ? Them : Others,
		theirSide == MapSide.T ? Others : Them,
		plant, null);

	/// <summary>A plant on <paramref name="site"/> at <paramref name="seconds"/>.</summary>
	public static DemoBombEvent Plant(DemoBombSite site, float seconds) => new(seconds, Them[0], "t11", 1, null) { Site = site };

	/// <summary>A radar position.</summary>
	public static DemoPosition At((float X, float Y) point) => new(0, 0, 0, point.X, point.Y);

	/// <summary>A kill with both positions; sides follow whether the ids are "them".</summary>
	public static DemoKill Kill(
		int round, long killer, long victim, float seconds, (float X, float Y)? killerAt = null, (float X, float Y)? victimAt = null,
		string weapon = "ak47", bool isOpening = false, MapSide theirSide = MapSide.T) =>
		new(round, seconds,
			new DemoKillParticipant(killer, $"p{killer}", SideOf(killer, theirSide), killerAt is { } k ? At(k) : null),
			new DemoKillParticipant(victim, $"p{victim}", SideOf(victim, theirSide), victimAt is { } v ? At(v) : null),
			null, weapon, false, false, false, false, false, false, isOpening, false);

	/// <summary>A grenade thrown by <paramref name="thrower"/> landing at <paramref name="landing"/>.</summary>
	public static DemoGrenade Grenade(int id, int round, long thrower, (float X, float Y) landing, float seconds = 20f, DemoGrenadeType type = DemoGrenadeType.Smoke) =>
		new(id, round, type, thrower, $"p{thrower}", MapSide.T, seconds, At(TSpawn), At(landing));

	/// <summary>A position track standing still at <paramref name="point"/> for seconds 0–30.</summary>
	public static DemoPlayerTrack Track(int round, long steamId, MapSide side, (float X, float Y) point, string weapon = "m4a1")
	{
		const int samples = 31;
		return new DemoPlayerTrack(
			round, steamId, side, 0,
			Enumerable.Repeat(new[] { 0, 0, 0 }, samples).SelectMany(x => x).ToList(),
			Enumerable.Repeat(new[] { point.X, point.Y }, samples).SelectMany(x => x).ToList(),
			Enumerable.Repeat(100, samples).ToList(),
			Enumerable.Repeat(weapon, samples).ToList());
	}

	/// <summary>A Mirage timeline.</summary>
	public static DemoTimeline Timeline(
		IReadOnlyList<DemoTimelineRound> rounds,
		IReadOnlyList<DemoKill>? kills = null,
		IReadOnlyList<DemoGrenade>? grenades = null,
		IReadOnlyList<DemoPlayerTrack>? positions = null,
		IReadOnlyList<DemoRoundEconomy>? economy = null) =>
		new DemoTimeline("de_mirage", MapName.Mirage, true, null, rounds, grenades ?? [])
		{
			Kills = kills ?? [],
			Positions = positions ?? [],
			Economy = economy ?? []
		};

	/// <summary>Their loadout for a round, every player with the same equipment value.</summary>
	public static DemoRoundEconomy Economy(int round, int perPlayer, MapSide theirSide = MapSide.T, string? primary = null) =>
		new(round, Them.Select(id => new DemoPlayerEconomy(id, $"p{id}", theirSide, perPlayer, 0, 0, 100, true, false, primary)).ToList());

	/// <summary>A T round's facts heading for <paramref name="target"/> at <paramref name="execSecond"/>.</summary>
	public static OpponentRoundFacts TRound(int number, MapArea? target, float? execSecond, bool won = true, string? buy = "Full", params GrenadeFact[] grenades) =>
		new(number, MapSide.T, won, buy,
			new OpponentTRoundFacts(target, execSecond, target is null ? null : new FactPoint(0.5f, 0.5f), target, execSecond, grenades.ToList()), null);

	/// <summary>A CT round's facts with the setup areas given.</summary>
	public static OpponentRoundFacts CtRound(int number, MapArea?[] setup, int earlyKills = 0, PostPlantBehaviour? postPlant = null, bool won = true, params AwpKillFact[] awpKills) =>
		new(number, MapSide.CT, won, "Full", null,
			new OpponentCtRoundFacts(setup.Select(a => new CtSetupSpot(0.5f, 0.5f, a, false)).ToList(), awpKills.ToList(), earlyKills, 0, postPlant));

	/// <summary>Facts of one demo.</summary>
	public static OpponentDemoFacts Facts(IEnumerable<OpponentRoundFacts> rounds, IEnumerable<OpponentPlayerFacts>? players = null) =>
		new(OpponentDemoFacts.CurrentVersion, rounds.ToList(), players?.ToList() ?? []);

	#endregion

	#region Private Methods

	private static MapSide SideOf(long steamId, MapSide theirSide)
	{
		var isThem = Them.Contains(steamId);
		var other = theirSide == MapSide.T ? MapSide.CT : MapSide.T;
		return isThem ? theirSide : other;
	}

	#endregion
}
