#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Abstractions;

/// <summary>One death in an official round. <paramref name="Killer"/> is null for world/fall/suicide deaths;
/// <paramref name="IsOpening"/> marks the round's first kill of an enemy (the opening duel) — trades, clutches and other
/// derived facts are computed later from this list in pure code, never by the collector.</summary>
public record DemoKill(
	int RoundNumber,
	float SecondsIntoRound,
	DemoKillParticipant? Killer,
	DemoKillParticipant Victim,
	DemoKillParticipant? Assister,
	string Weapon,
	bool Headshot,
	bool Wallbang,
	bool ThroughSmoke,
	bool NoScope,
	bool AttackerBlind,
	bool AssistedFlash,
	bool IsOpening,
	bool IsTeamKill);

/// <summary>A player taking part in a kill, with their side at that moment and (for killer/victim) where they stood.</summary>
public record DemoKillParticipant(long SteamId64, string Name, MapSide? Side, DemoPosition? Position);

/// <summary>Everyone's loadout in one round once the buy phase is over.</summary>
public record DemoRoundEconomy(int RoundNumber, IReadOnlyList<DemoPlayerEconomy> Players);

/// <summary>One player's loadout for a round: equipment value (the game's own valuation of everything carried), money
/// left and spent, armor/helmet/defuser and the primary weapon (null when only a pistol was carried).</summary>
public record DemoPlayerEconomy(
	long SteamId64,
	string Name,
	MapSide Side,
	int EquipmentValue,
	int Money,
	int MoneySpent,
	int Armor,
	bool HasHelmet,
	bool HasDefuser,
	string? PrimaryWeapon);
