using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tactics.GetTacticDetail;

/// <summary>Returns one tactic together with its full radar layout.</summary>
public record GetTacticDetailQuery(Guid TacticId) : IRequest<ErrorOr<TacticDetailDto>>;
