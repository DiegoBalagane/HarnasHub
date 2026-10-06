#region Usings

using ErrorOr;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentDemoReplay;

/// <summary>Produces the 2D replay of one round of an analysed opponent demo.</summary>
public record GetOpponentDemoReplayQuery(Guid OpponentDemoId, int RoundNumber) : IRequest<ErrorOr<RoundReplayDto>>;
