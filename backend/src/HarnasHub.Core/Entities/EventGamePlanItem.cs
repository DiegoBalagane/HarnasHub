using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A tactic or analysis board attached to an event's game plan. <see cref="TargetId"/> is a soft reference:
/// when the target is deleted the item is simply skipped on read.</summary>
public class EventGamePlanItem
{
	public Guid Id { get; set; }
	public Guid EventId { get; set; }
	public GamePlanItemKind Kind { get; set; }
	public Guid TargetId { get; set; }
	/// <summary>1-based position within items of the same kind, as the coach ordered them.</summary>
	public int Order { get; set; }
}
