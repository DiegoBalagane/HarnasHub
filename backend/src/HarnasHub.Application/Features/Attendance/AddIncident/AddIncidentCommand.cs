using ErrorOr;
using HarnasHub.Application.Features.Attendance.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Attendance.AddIncident;

/// <summary>Logs a lateness or absence for a player on a training day. Coach/Manager only.</summary>
public record AddIncidentCommand(Guid UserId, AttendanceIncidentType Type, DateOnly OccurredOn, string? Note)
	: IRequest<ErrorOr<AttendanceIncidentDto>>;
