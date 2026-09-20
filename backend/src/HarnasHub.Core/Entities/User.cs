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
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
