using ErrorOr;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.GetEventAvailability;

/// <summary>Returns every team member's availability for one event ("NotSet" if not declared).</summary>
public record GetEventAvailabilityQuery(Guid EventId) : IRequest<ErrorOr<List<MemberAvailabilityDto>>>;
