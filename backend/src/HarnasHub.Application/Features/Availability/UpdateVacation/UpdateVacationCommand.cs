using ErrorOr;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Availability.UpdateVacation;

/// <summary>Edits an existing time-off range in place, instead of forcing a delete-and-recreate. Allowed for its owner, or any Coach/Manager.</summary>
public record UpdateVacationCommand(
	Guid VacationId,
	DateOnly StartDate,
	DateOnly EndDate,
	string? Reason) : IRequest<ErrorOr<VacationDto>>;
