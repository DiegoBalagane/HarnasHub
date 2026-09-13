using ErrorOr;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.SetAvailability;

/// <summary>Declares (or updates) the current user's availability for an event.</summary>
public record SetAvailabilityCommand(Guid EventId, AvailabilityStatus Status) : IRequest<ErrorOr<Success>>;
