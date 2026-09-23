namespace HarnasHub.Application.Features.Attendance.Shared;

/// <summary>One logged lateness/absence entry.</summary>
public record AttendanceIncidentDto(
	Guid Id,
	Guid UserId,
	string PlayerName,
	string Type,
	DateOnly OccurredOn,
	string? Note,
	DateTime CreatedAtUtc);

/// <summary>A roster player's total lateness/absence counts, for the team-wide overview every member can see.</summary>
public record AttendanceSummaryEntryDto(Guid UserId, string PlayerName, int LateCount, int AbsentCount);
