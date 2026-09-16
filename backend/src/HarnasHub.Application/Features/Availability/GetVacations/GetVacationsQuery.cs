using ErrorOr;
using HarnasHub.Application.Features.Availability.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Availability.GetVacations;

/// <summary>Returns the current user's own time-off ranges, newest first.</summary>
public record GetVacationsQuery : IRequest<ErrorOr<List<VacationDto>>>;
