using ErrorOr;
using HarnasHub.Application.Features.GamePlan.Shared;
using MediatR;

namespace HarnasHub.Application.Features.GamePlan.SetEventGamePlan;

/// <summary>Replaces an event's whole game plan; list order is display order. Coach/Manager only — enforced at the endpoint.</summary>
public record SetEventGamePlanCommand(Guid EventId, string? Notes, List<Guid> TacticIds, List<Guid> BoardIds)
	: IRequest<ErrorOr<EventGamePlanDto>>;
