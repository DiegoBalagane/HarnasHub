using ErrorOr;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.CreateEvent;

/// <summary>Creates a new calendar event. Coach/Manager only — enforced at the endpoint.</summary>
public record CreateEventCommand(
	string Title,
	EventType Type,
	DateTime StartsAtUtc,
	string? Location,
	string? Notes) : IRequest<ErrorOr<EventDto>>;
