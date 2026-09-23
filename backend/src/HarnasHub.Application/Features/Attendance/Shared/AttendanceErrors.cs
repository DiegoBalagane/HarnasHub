using ErrorOr;

namespace HarnasHub.Application.Features.Attendance.Shared;

/// <summary>Domain errors for the Attendance feature slice.</summary>
public static class AttendanceErrors
{
	public static Error IncidentNotFound => Error.NotFound(
		"Attendance.IncidentNotFound",
		"Nie znaleziono wpisu.");

	public static Error PlayerNotFound => Error.NotFound(
		"Attendance.PlayerNotFound",
		"Nie znaleziono zawodnika.");
}
