namespace HarnasHub.Core.Entities;

/// <summary>The outcome of a scrim, match, or tournament game, with an optional link to the demo file.</summary>
public class MatchResult
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Opponent { get; set; } = string.Empty;
	public int OurScore { get; set; }
	public int OpponentScore { get; set; }
	public string? MapName { get; set; }
	public string? DemoUrl { get; set; }
	public string? Notes { get; set; }
	public DateTime PlayedAtUtc { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
