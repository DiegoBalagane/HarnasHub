using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>One lineup entry in the team's per-map grenade library.</summary>
public class NadeEntry
{
	#region Public Properties

	public Guid Id { get; set; }
	public MapName MapName { get; set; }
	public GrenadeType Type { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? YoutubeUrl { get; set; }
	/// <summary>Radar-relative landing-spot fraction in [0,1], measured from the left edge; null when the entry has no pin yet.</summary>
	public float? LandingX { get; set; }
	/// <summary>Radar-relative landing-spot fraction in [0,1], measured from the top edge; null when the entry has no pin yet.</summary>
	public float? LandingY { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
