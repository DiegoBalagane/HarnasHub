using ErrorOr;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Availability.SetVacation;

/// <summary>Adds a time-off range for the current user.</summary>
public record SetVacationCommand(DateOnly StartDate, DateOnly EndDate, string? Reason) : IRequest<ErrorOr<VacationDto>>;
