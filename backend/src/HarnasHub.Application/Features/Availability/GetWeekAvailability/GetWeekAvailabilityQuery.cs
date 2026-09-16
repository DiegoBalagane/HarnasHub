using ErrorOr;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Availability.GetWeekAvailability;

/// <summary>Returns the effective availability of every team member for the seven days starting at <paramref name="WeekStart"/>.</summary>
public record GetWeekAvailabilityQuery(DateOnly WeekStart) : IRequest<ErrorOr<WeekAvailabilityDto>>;
