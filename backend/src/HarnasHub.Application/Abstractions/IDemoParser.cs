using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Abstractions;

/// <summary>Reads a CS2 demo (.dem) stream and folds it into per-player kill/death/damage totals. Implemented in Infrastructure
/// against a third-party parsing library, so Application stays free of that dependency — same inversion pattern as everything else here.</summary>
public interface IDemoParser
{
	/// <summary>Reads the whole demo with only the stats collector enabled and returns its per-player totals.</summary>
	Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken);

	/// <summary>Reads the whole demo once, running only the collectors <paramref name="options"/> asks for, and returns
	/// the combined timeline — sections belonging to disabled collectors are left null/empty.</summary>
	Task<DemoTimeline> ParseAsync(Stream demoStream, DemoParseOptions options, CancellationToken cancellationToken);
}

/// <summary>Which collectors a single pass over a demo should run; a flags enum so later stages (economy, kills,
/// position sampling) only add a value instead of changing any existing caller.</summary>
[Flags]
public enum DemoCollectors
{
	None = 0,
	Stats = 1,
	Rounds = 2,
	Grenades = 4,
	Economy = 8,
	Kills = 16,
	Blinds = 32,
	Positions = 64
}

/// <summary>Options for one parse — callers only pay for the collectors they actually need.</summary>
public record DemoParseOptions(DemoCollectors Collectors)
{
	/// <summary>Only the per-player totals, i.e. what the original stats/score import has always computed.</summary>
	public static DemoParseOptions StatsOnly { get; } = new(DemoCollectors.Stats);

	/// <summary>Rounds plus every grenade, used by the tactic-from-demo import.</summary>
	public static DemoParseOptions RoundsAndGrenades { get; } = new(DemoCollectors.Rounds | DemoCollectors.Grenades);

	/// <summary>Everything a stored match timeline contains: stats, rounds, grenades, economy and kills.</summary>
	public static DemoParseOptions FullTimeline { get; } = new(
		DemoCollectors.Stats | DemoCollectors.Rounds | DemoCollectors.Grenades | DemoCollectors.Economy | DemoCollectors.Kills | DemoCollectors.Blinds);

	/// <summary>Everything in <see cref="FullTimeline"/> plus per-second position samples — what our own match timelines store
	/// since parser version 4 (2D round replay, tactic matching). Trade-off: positions add roughly 10 players x ~100 samples x
	/// ~24 rounds of small ints/floats, which grows the gzip file by a few hundred KB (about 2-3x a timeline without them).</summary>
	public static DemoParseOptions MatchAnalysis { get; } = new(FullTimeline.Collectors | DemoCollectors.Positions);

	/// <summary>Same collectors as <see cref="MatchAnalysis"/> — opponent tendencies (CT setups, entries) need positions too.</summary>
	public static DemoParseOptions OpponentAnalysis => MatchAnalysis;

	/// <summary>Whether the given collector is enabled.</summary>
	public bool Includes(DemoCollectors collector) => (Collectors & collector) == collector;
}

/// <summary>Everything extracted from one demo: how many rounds were played, which map (null if the demo didn't say,
/// or said a map outside the current pool), each participant's raw totals, and the per-round winner/side breakdown.</summary>
public record DemoParseResult(
	int RoundsPlayed,
	MapName? MapName,
	IReadOnlyList<DemoPlayerStats> Players,
	IReadOnlyList<DemoRoundResult> Rounds);

/// <summary>Who won one round and which SteamID64s stood on each side of it — the raw material for working out the
/// match score, since a demo never labels either team as "ours" and both swap sides at halftime.</summary>
public record DemoRoundResult(
	MapSide WinnerSide,
	IReadOnlyList<long> TerroristSteamIds,
	IReadOnlyList<long> CounterTerroristSteamIds);

/// <summary>One player's raw totals from a parsed demo — not yet turned into ADR/HS%/rating/KAST%, that's the caller's job.
/// <paramref name="KastRounds"/> is how many rounds this player got a Kill/Assist, Survived, or was Traded.
/// <paramref name="MultiKillRounds"/> is keyed by kill count (2..5) within a single round, e.g. <c>MultiKillRounds[4]</c> = number of 4-kill rounds.</summary>
public record DemoPlayerStats(
	long SteamId64,
	string PlayerName,
	int Kills,
	int Deaths,
	int Assists,
	int Headshots,
	int DamageDealt,
	int EntryKills,
	int EntryDeaths,
	int KastRounds,
	int UtilityDamage,
	int FlashAssists,
	IReadOnlyDictionary<int, int> MultiKillRounds,
	IReadOnlyList<DemoDeathPosition> DeathPositions);

/// <summary>Where and for which side a player died, as a radar-relative fraction in [0,1] — same convention as
/// <c>MapPositionAssignment.X/Y</c> — null when the demo's map isn't in the current pool, so no calibration exists to convert it.</summary>
public record DemoDeathPosition(float X, float Y, MapSide Side);
