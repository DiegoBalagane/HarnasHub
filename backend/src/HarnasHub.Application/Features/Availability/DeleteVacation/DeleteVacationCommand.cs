using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Availability.DeleteVacation;

/// <summary>Deletes a vacation. Allowed for its owner, or any Coach/Manager.</summary>
public record DeleteVacationCommand(Guid VacationId) : IRequest<ErrorOr<Success>>;
