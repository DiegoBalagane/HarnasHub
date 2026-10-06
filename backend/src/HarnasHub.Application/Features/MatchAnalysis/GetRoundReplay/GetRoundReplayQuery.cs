#region Usings

using ErrorOr;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetRoundReplay;

/// <summary>Produces the 2D replay of one round of one of our matches from its stored timeline.</summary>
public record GetRoundReplayQuery(Guid MatchResultId, int RoundNumber) : IRequest<ErrorOr<RoundReplayDto>>;
