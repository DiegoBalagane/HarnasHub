using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.DeleteEvent;

/// <summary>Deletes a calendar event. Coach/Manager only — enforced at the endpoint.</summary>
public record DeleteEventCommand(Guid EventId) : IRequest<ErrorOr<Success>>;
