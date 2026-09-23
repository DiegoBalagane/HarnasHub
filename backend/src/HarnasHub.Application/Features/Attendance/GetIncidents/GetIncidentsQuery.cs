using ErrorOr;
using HarnasHub.Application.Features.Attendance.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Attendance.GetIncidents;

/// <summary>Lists logged attendance incidents, newest first, optionally narrowed to one player.</summary>
public record GetIncidentsQuery(Guid? UserId) : IRequest<ErrorOr<List<AttendanceIncidentDto>>>;
