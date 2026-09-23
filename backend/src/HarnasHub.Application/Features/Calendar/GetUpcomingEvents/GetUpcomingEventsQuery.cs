using ErrorOr;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.GetUpcomingEvents;

/// <summary>Returns every event from now onward, soonest first — or, when <paramref name="IncludePast"/> is set,
/// every event ever logged, most recent first, so a past one can still be looked up.</summary>
public record GetUpcomingEventsQuery(bool IncludePast = false) : IRequest<ErrorOr<List<EventDto>>>;
