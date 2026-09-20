namespace HarnasHub.Core.Entities;

/// <summary>A named tournament that groups several <see cref="MatchResult"/> rows together.</summary>
public class Tournament
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
