using ErrorOr;
using HarnasHub.Application.Features.Veto.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Veto.GetEventVeto;

/// <summary>Returns the veto recorded for an event, in order; empty when none was recorded yet.</summary>
public record GetEventVetoQuery(Guid EventId) : IRequest<ErrorOr<List<VetoStepDto>>>;
