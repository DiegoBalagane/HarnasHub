#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;

/// <summary>Whether a replay can draw positions: they exist and the map's radar fit is verified, the timeline was parsed
/// without position sampling, or the map has no verified radar fit (Dust2/Inferno/Nuke today) — the last two still get kills.</summary>
public enum ReplayPositionsStatus
{
	Available = 0,
	NotRecorded = 1,
	UncalibratedMap = 2
}

/// <summary>Which team a replayed player belongs to from the viewer's perspective.</summary>
public enum ReplayTeam
{
	Unknown = 0,
	Ours = 1,
	Opponent = 2
}

/// <summary>A compact 2D replay of one round: players indexed once, then one frame per whole second from freeze end
/// (<c>Frames[s].Second == s</c>, so the client indexes directly and interpolates between neighbours), plus kills,
/// grenades and the bomb. Coordinates are radar fractions in [0,1] and are null/empty unless
/// <paramref name="PositionsStatus"/> is <see cref="ReplayPositionsStatus.Available"/>.</summary>
public record RoundReplayDto(
	string? MapName,
	int RoundNumber,
	int RoundsCount,
	string? OpponentName,
	ReplayPositionsStatus PositionsStatus,
	int DurationSeconds,
	MapSide? WinnerSide,
	DemoRoundEndReason EndReason,
	MapSide? OurSide,
	IReadOnlyList<ReplayPlayerDto> Players,
	IReadOnlyList<ReplayFrameDto> Frames,
	IReadOnlyList<ReplayKillDto> Kills,
	IReadOnlyList<ReplayGrenadeDto> Grenades,
	ReplayBombDto? Bomb);

/// <summary>A participant of the round; <paramref name="Index"/> is what frames, kills and grenades refer to.</summary>
public record ReplayPlayerDto(int Index, string SteamId64, string Name, MapSide Side, ReplayTeam Team);

/// <summary>Every alive, sampled player at one whole second.</summary>
public record ReplayFrameDto(int Second, IReadOnlyList<ReplayPlayerStateDto> Players);

/// <summary>One player's state in a frame: radar position, health and active weapon (null when unknown).</summary>
public record ReplayPlayerStateDto(int Player, float X, float Y, int Health, string? Weapon);

/// <summary>A kill for the feed and the death marker; <paramref name="Killer"/> is null for world damage, the position is
/// where the victim died.</summary>
public record ReplayKillDto(
	float Second,
	int? Killer,
	int Victim,
	string Weapon,
	bool Headshot,
	bool IsTeamKill,
	float? X,
	float? Y);

/// <summary>One grenade from throw to the end of its effect. <paramref name="TimesApproximate"/> is true when the
/// detonation wasn't recorded (older timelines, unmatched fires) and typical fuse times were used instead; the effect
/// length (smoke ~18 s, fire ~7 s) is always the game's nominal value, not measured.</summary>
public record ReplayGrenadeDto(
	int Id,
	DemoGrenadeType Type,
	int? Thrower,
	MapSide? Side,
	float ThrowSecond,
	float DetonateSecond,
	float EndSecond,
	bool TimesApproximate,
	float? ThrowX,
	float? ThrowY,
	float? LandX,
	float? LandY);

/// <summary>Bomb plant of the round with its defuse or explosion, if any.</summary>
public record ReplayBombDto(
	float PlantSecond,
	DemoBombSite? Site,
	int? Planter,
	float? X,
	float? Y,
	float? DefuseSecond,
	int? Defuser,
	float? ExplodeSecond);
