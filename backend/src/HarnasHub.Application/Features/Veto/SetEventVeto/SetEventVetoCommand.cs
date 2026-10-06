using ErrorOr;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Veto.SetEventVeto;

/// <summary>One veto step as entered by the coach; its position in the list is its order.</summary>
public record VetoStepInput(VetoActor Actor, VetoAction Action, MapName MapName);

/// <summary>Replaces the whole veto recorded for an event (an empty list clears it). Coach/Manager only — enforced at the endpoint.</summary>
public record SetEventVetoCommand(Guid EventId, List<VetoStepInput> Steps) : IRequest<ErrorOr<List<VetoStepDto>>>;
