using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>One player's availability declaration for one <see cref="Event"/>.</summary>
public class Availability
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid EventId { get; set; }
	public Guid UserId { get; set; }
	public AvailabilityStatus Status { get; set; }
	public DateTime UpdatedAtUtc { get; set; }

	#endregion
}
