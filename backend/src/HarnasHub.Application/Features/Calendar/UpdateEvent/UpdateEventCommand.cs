using ErrorOr;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.UpdateEvent;

/// <summary>Edits an existing calendar event in place — e.g. to fix a wrong time. Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateEventCommand(
	Guid EventId,
	string Title,
	EventType Type,
	DateTime StartsAtUtc,
	DateTime? EndsAtUtc,
	string? Location,
	string? Url,
	string? Notes) : IRequest<ErrorOr<EventDto>>;
