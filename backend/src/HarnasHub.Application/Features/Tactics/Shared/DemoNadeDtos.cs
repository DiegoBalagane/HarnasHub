#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared;

/// <summary>Every round of a parsed demo with its grenades, so the import wizard can switch rounds/sides without re-parsing.</summary>
public record DemoNadesDto(MapName MapName, IReadOnlyList<DemoNadeRoundDto> Rounds);

/// <summary>One round: who won it and the score after it, expressed per side (team currently on T vs team currently on CT).</summary>
public record DemoNadeRoundDto(
	int Number,
	MapSide? WinnerSide,
	int TerroristScore,
	int CounterTerroristScore,
	IReadOnlyList<DemoNadeDto> Grenades);

/// <summary>One grenade thrown in a round; positions are radar fractions in [0,1], <paramref name="ThrowerSteamId"/> is a
/// string because SteamID64 exceeds JavaScript's safe-integer range.</summary>
public record DemoNadeDto(
	int Id,
	GrenadeType Type,
	string ThrowerName,
	string? ThrowerSteamId,
	MapSide Side,
	float ThrowX,
	float ThrowY,
	float LandX,
	float LandY,
	float SecondsIntoRound);

/// <summary>One grenade the coach selected in the wizard, as submitted for saving into a tactic.</summary>
public record ImportedNadeInput(
	GrenadeType Type,
	string ThrowerName,
	float ThrowX,
	float ThrowY,
	float LandX,
	float LandY,
	float SecondsIntoRound);
