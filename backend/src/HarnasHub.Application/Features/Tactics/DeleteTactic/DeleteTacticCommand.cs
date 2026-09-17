using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tactics.DeleteTactic;

/// <summary>Deletes a tactic and its radar points. Coach/Manager only.</summary>
public record DeleteTacticCommand(Guid TacticId) : IRequest<ErrorOr<Success>>;
