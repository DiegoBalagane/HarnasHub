namespace HarnasHub.Core.Entities;

/// <summary>The coach's written plan for one event ("what we play on Friday"); at most one per event.</summary>
public class EventGamePlan
{
	public Guid Id { get; set; }
	public Guid EventId { get; set; }
	public string? Notes { get; set; }
	public Guid UpdatedByUserId { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
}
