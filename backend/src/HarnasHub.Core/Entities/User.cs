using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A team member account — player, coach, or manager.</summary>
public class User
{
    #region Public Properties

    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    #endregion
}
