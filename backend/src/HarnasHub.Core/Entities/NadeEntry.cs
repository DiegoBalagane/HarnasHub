using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>One lineup entry in the team's per-map grenade library.</summary>
public class NadeEntry
{
    #region Public Properties

    public Guid Id { get; set; }
    public string MapName { get; set; } = string.Empty;
    public GrenadeType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? YoutubeUrl { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    #endregion
}
