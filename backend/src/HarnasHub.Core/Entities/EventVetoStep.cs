using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>One step of the map veto recorded for a match-like event, in the order it happened.</summary>
public class EventVetoStep
{
	public Guid Id { get; set; }
	public Guid EventId { get; set; }
	/// <summary>1-based position in the veto sequence.</summary>
	public int Order { get; set; }
	public VetoActor Actor { get; set; }
	public VetoAction Action { get; set; }
	public MapName MapName { get; set; }
}
