using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>One player's availability declaration for one calendar day.</summary>
public class PlayerAvailabilityDay
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public DateOnly Date { get; set; }
	public DayAvailabilityStatus Status { get; set; }
	public TimeOnly? AvailableFromLocal { get; set; }
	public TimeOnly? AvailableToLocal { get; set; }
	public string? Note { get; set; }
	public DateTime UpdatedAtUtc { get; set; }

	#endregion
}
