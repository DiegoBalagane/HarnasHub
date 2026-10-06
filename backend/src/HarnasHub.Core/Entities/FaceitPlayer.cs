namespace HarnasHub.Core.Entities;

/// <summary>Cached FACEIT player profile — ours (matched by SteamID64) or an opponent's.</summary>
public class FaceitPlayer
{
	#region Public Properties

	/// <summary>FACEIT player id (UUID string) — the primary key.</summary>
	public string Id { get; set; } = string.Empty;
	public string Nickname { get; set; } = string.Empty;
	/// <summary>SteamID64 of the CS2 account linked on FACEIT, used to match our roster via <see cref="User.SteamId64"/>.</summary>
	public string? SteamId64 { get; set; }
	public int? Elo { get; set; }
	public int? SkillLevel { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
	/// <summary>When the player's match history was last pulled; null until the first sync.</summary>
	public DateTime? HistorySyncedAtUtc { get; set; }
	/// <summary>Lifetime per-map CS2 stats from FACEIT ("Map" segments of <c>/players/{id}/stats/cs2</c>) as JSON; null until fetched.</summary>
	public string? MapStatsJson { get; set; }
	/// <summary>When <see cref="MapStatsJson"/> was last pulled; refreshed on the profile cadence.</summary>
	public DateTime? MapStatsSyncedAtUtc { get; set; }

	#endregion
}
