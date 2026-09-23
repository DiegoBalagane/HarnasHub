#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Mutable running totals for one player while a demo is being read; converted to the immutable
/// <c>DemoPlayerStats</c> once the whole file has been parsed.</summary>
internal sealed class DemoPlayerAccumulator(ulong steamId64, string playerName)
{
	#region Public Properties

	public ulong SteamId64 { get; } = steamId64;
	public string PlayerName { get; } = playerName;
	public int Kills { get; set; }
	public int Deaths { get; set; }
	public int Assists { get; set; }
	public int Headshots { get; set; }
	public int DamageDealt { get; set; }
	public int UtilityDamage { get; set; }
	public int EntryKills { get; set; }
	public int EntryDeaths { get; set; }
	public int KastRounds { get; set; }
	public int FlashAssists { get; set; }
	public Dictionary<int, int> MultiKillRounds { get; } = new() { [2] = 0, [3] = 0, [4] = 0, [5] = 0 };
	public List<(float WorldX, float WorldY, MapSide Side)> RawDeathPositions { get; } = [];

	#endregion
}
