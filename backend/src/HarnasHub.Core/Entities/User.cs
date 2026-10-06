using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A team member account — player, coach, or manager. Identity comes from Discord OAuth, no local password.</summary>
public class User
{
	#region Public Properties

	public Guid Id { get; set; }
	public string DiscordId { get; set; } = string.Empty;
	public string DisplayName { get; set; } = string.Empty;
	public string? AvatarUrl { get; set; }
	public AccessLevel AccessLevel { get; set; }
	public bool IsCoach { get; set; }
	public TeamRole? TeamRole { get; set; }
	public RosterSlot? RosterSlot { get; set; }
	public PinColor? PinColor { get; set; }
	public string? PinMark { get; set; }
	public string? InGameNickname { get; set; }
	/// <summary>Self-reported SteamID64, used to match this player to a CS2 demo's participants when importing stats.
	/// A string, not a number — like <see cref="DiscordId"/>, it exceeds Number.MAX_SAFE_INTEGER and would lose precision in JS/JSON.</summary>
	public string? SteamId64 { get; set; }
	/// <summary>Whether the player appears in team stats (leaderboard, opponent report "our" side, dashboard rankings); hiding never deletes data.</summary>
	public bool ShowInStats { get; set; } = true;
	/// <summary>Whether the player appears in the availability calendar, daily status lists and their counts; hiding never deletes data.</summary>
	public bool ShowInCalendar { get; set; } = true;
	/// <summary>Manually set FACEIT nickname for players whose SteamID64 is not linked to FACEIT (or who have none); used as a fallback lookup.</summary>
	public string? FaceitNickname { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
