#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Abstractions;

/// <summary>Everything one pass over a demo produced. Each section belongs to one collector and is null/empty when that
/// collector wasn't requested in <see cref="DemoParseOptions"/>; events are flat lists keyed by
/// <see cref="DemoTimelineRound.Number"/>, so a new collector adds a list here without touching existing ones.</summary>
public record DemoTimeline(
	string? RawMapName,
	MapName? MapName,
	bool IsRadarCalibrated,
	DemoParseResult? Stats,
	IReadOnlyList<DemoTimelineRound> Rounds,
	IReadOnlyList<DemoGrenade> Grenades)
{
	/// <summary>Every death in an official round (kill collector); init-only so later sections never change the constructor.</summary>
	public IReadOnlyList<DemoKill> Kills { get; init; } = [];

	/// <summary>Per-round loadouts at the end of the buy phase (economy collector).</summary>
	public IReadOnlyList<DemoRoundEconomy> Economy { get; init; } = [];

	/// <summary>Every flash blinding a player in an official round (blind collector).</summary>
	public IReadOnlyList<DemoBlind> Blinds { get; init; } = [];

	/// <summary>Per-second position tracks of every alive player (position sampler; empty unless requested).</summary>
	public IReadOnlyList<DemoPlayerTrack> Positions { get; init; } = [];
}

/// <summary>A world-space position from the demo, plus its radar-relative fraction in [0,1] (same convention as
/// <c>MapPositionAssignment.X/Y</c>) — the radar part is null when the map has no calibration.</summary>
public record DemoPosition(float WorldX, float WorldY, float WorldZ, float? RadarX, float? RadarY);

/// <summary>Why a round ended, collapsed from the game's own (mostly hostage/VIP-mode) reason list to what matters in competitive play.</summary>
public enum DemoRoundEndReason
{
	Other = 0,
	BombExploded = 1,
	BombDefused = 2,
	Elimination = 3,
	TimeExpired = 4,
	Surrender = 5
}

/// <summary>One official (non-warmup, non-knife) round. <paramref name="Number"/> is 1-based within the match;
/// times are seconds of game time, <paramref name="FreezeEndTime"/> is null when the recording began after freeze time ended.</summary>
public record DemoTimelineRound(
	int Number,
	float? StartTime,
	float? FreezeEndTime,
	float EndTime,
	MapSide? WinnerSide,
	DemoRoundEndReason EndReason,
	IReadOnlyList<long> TerroristSteamIds,
	IReadOnlyList<long> CounterTerroristSteamIds,
	DemoBombEvent? BombPlant,
	DemoBombEvent? BombDefuse);

/// <summary>A bomb plant or defuse. <paramref name="SiteEntityIndex"/> is the raw bomb-target entity index from the game
/// event; <see cref="Site"/> is its A/B letter when the planted bomb entity reported one, otherwise consumers fall back
/// to map zones (<c>MapZones</c>) on <paramref name="Position"/>, which is where the player stood.</summary>
public record DemoBombEvent(
	float SecondsIntoRound,
	long? PlayerSteamId64,
	string? PlayerName,
	int SiteEntityIndex,
	DemoPosition? Position)
{
	/// <summary>Bomb site as reported by the planted C4 entity; null when it wasn't seen.</summary>
	public DemoBombSite? Site { get; init; }
}

/// <summary>The two bomb sites of a defusal map.</summary>
public enum DemoBombSite
{
	A = 0,
	B = 1
}

/// <summary>Grenade kinds as the demo distinguishes them — finer than <see cref="GrenadeType"/>, which has no decoy and
/// merges molotov with incendiary.</summary>
public enum DemoGrenadeType
{
	Smoke = 0,
	Flash = 1,
	HighExplosive = 2,
	Molotov = 3,
	Incendiary = 4,
	Decoy = 5
}

/// <summary>One thrown grenade. <paramref name="Id"/> is unique within the timeline; <paramref name="SecondsIntoRound"/>
/// counts from freeze end (or round start when freeze end wasn't recorded). <paramref name="Landing"/> is the detonation
/// point (smoke/flash/HE/decoy event, inferno start for fire) and falls back to the projectile's last position, so it is
/// only null when the projectile was never seen moving.</summary>
public record DemoGrenade(
	int Id,
	int RoundNumber,
	DemoGrenadeType Type,
	long? ThrowerSteamId64,
	string ThrowerName,
	MapSide? ThrowerSide,
	float SecondsIntoRound,
	DemoPosition Throw,
	DemoPosition? Landing)
{
	/// <summary>Seconds into the round of the detonation (burn start for fire, effect start for a decoy); null when no
	/// detonation event was linked to the projectile or the timeline predates parser version 4.</summary>
	public float? DetonationSecond { get; init; }
}
