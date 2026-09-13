namespace HarnasHub.Core.Entities;

/// <summary>Scouting notes about an opponent, prepared ahead of a match.</summary>
public class OpponentNote
{
    #region Public Properties

    public Guid Id { get; set; }
    public string OpponentName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MaterialUrl { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    #endregion
}
