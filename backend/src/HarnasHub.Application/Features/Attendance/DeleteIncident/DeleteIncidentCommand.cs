using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Attendance.DeleteIncident;

/// <summary>Removes a logged attendance incident. Coach/Manager only.</summary>
public record DeleteIncidentCommand(Guid IncidentId) : IRequest<ErrorOr<Success>>;
