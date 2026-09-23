using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A logged lateness or absence for a player on a training day, recorded by a coach or manager.</summary>
public class AttendanceIncident
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public AttendanceIncidentType Type { get; set; }
	public DateOnly OccurredOn { get; set; }
	public string? Note { get; set; }
	public Guid RecordedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
}
