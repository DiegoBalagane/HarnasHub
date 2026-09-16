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
	public UserRole Role { get; set; }
	public TeamRole? TeamRole { get; set; }
	public string? InGameNickname { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
