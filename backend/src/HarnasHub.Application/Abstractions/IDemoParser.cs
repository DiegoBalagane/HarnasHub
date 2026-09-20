using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Abstractions;

/// <summary>Reads a CS2 demo (.dem) stream and folds it into per-player kill/death/damage totals. Implemented in Infrastructure
/// against a third-party parsing library, so Application stays free of that dependency — same inversion pattern as everything else here.</summary>
public interface IDemoParser
{
	Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken);
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
