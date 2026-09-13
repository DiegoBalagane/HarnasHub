using ErrorOr;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.GetUpcomingEvents;

/// <summary>Returns every event from now onward, soonest first.</summary>
public record GetUpcomingEventsQuery : IRequest<ErrorOr<List<EventDto>>>;
