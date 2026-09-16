namespace HarnasHub.Core.Entities;

/// <summary>A categorized learning resource (link) shared with the team.</summary>
public class TrainingMaterial
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Url { get; set; } = string.Empty;
	public string? Category { get; set; }
	public string? Description { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
