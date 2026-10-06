#region Usings

using DemoFile.Game.Cs;
using DemoFile.GameStatic;

#endregion

namespace HarnasHub.Infrastructure.Demos.Collectors;

/// <summary>Weapon naming/classification helpers: turns a weapon entity into the same short name the game uses in
/// kill events (e.g. "ak47", "awp", "usp_silencer") and picks a player's primary weapon.</summary>
internal static class DemoWeapons
{
	#region Private Fields

	private static readonly HashSet<string> Pistols =
	[
		"glock", "hkp2000", "usp_silencer", "p250", "elite", "fiveseven", "tec9", "cz75a", "deagle", "revolver"
	];

	private static readonly HashSet<string> NonPrimary =
	[
		"c4", "taser", "flashbang", "hegrenade", "smokegrenade", "molotov", "incgrenade", "decoy", "healthshot"
	];

	#endregion

	#region Public Methods

	/// <summary>Short weapon name, via the item definition (authoritative) or the entity's server class as a fallback.</summary>
	public static string? Name(CCSWeaponBase weapon)
	{
		if (GameItems.ItemDefinitions.TryGetValue(weapon.EconItem.ItemDefinitionIndex, out var definition)
			&& !string.IsNullOrWhiteSpace(definition.Name))
		{
			return Normalize(definition.Name);
		}

		var className = weapon.ServerClass.Name;
		return string.IsNullOrWhiteSpace(className) ? null : Normalize(className.TrimStart('C').Replace("Weapon", "", StringComparison.Ordinal));
	}

	/// <summary>The most valuable-looking primary (rifle/SMG/shotgun/MG) a pawn carries, or null when only pistols/knives/utility.</summary>
	public static string? Primary(CCSPlayerPawn pawn) =>
		pawn.Weapons
			.Select(Name)
			.FirstOrDefault(name => name is not null && IsPrimary(name));

	/// <summary>Whether a short weapon name is a primary weapon.</summary>
	public static bool IsPrimary(string name) =>
		!Pistols.Contains(name) && !NonPrimary.Contains(name) && !name.Contains("knife", StringComparison.Ordinal) && name != "bayonet";

	#endregion

	#region Private Methods

	private static string Normalize(string name)
	{
		var lower = name.Trim().ToLowerInvariant();
		return lower.StartsWith("weapon_", StringComparison.Ordinal) ? lower["weapon_".Length..] : lower;
	}

	#endregion
}
