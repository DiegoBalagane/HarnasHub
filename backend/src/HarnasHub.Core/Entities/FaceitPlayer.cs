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

	#endregion
}
