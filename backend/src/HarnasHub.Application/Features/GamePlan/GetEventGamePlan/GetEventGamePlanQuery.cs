using ErrorOr;
using HarnasHub.Application.Features.GamePlan.Shared;
using MediatR;

namespace HarnasHub.Application.Features.GamePlan.GetEventGamePlan;

/// <summary>Returns the game plan of an event — empty (no notes, no items) when none was written yet.</summary>
public record GetEventGamePlanQuery(Guid EventId) : IRequest<ErrorOr<EventGamePlanDto>>;
