using ErrorOr;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Availability.SetDayAvailability;

/// <summary>Declares (or updates) the current user's availability for a single day.</summary>
public record SetDayAvailabilityCommand(
	DateOnly Date,
	DayAvailabilityStatus Status,
	TimeOnly? AvailableFromLocal,
	TimeOnly? AvailableToLocal,
	string? Note) : IRequest<ErrorOr<Success>>;
