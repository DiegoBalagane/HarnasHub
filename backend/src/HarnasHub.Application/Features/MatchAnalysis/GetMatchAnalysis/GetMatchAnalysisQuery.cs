#region Usings

using ErrorOr;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMatchAnalysis;

/// <summary>Produces the deep analysis (trades, clutches, opening duels, flashes, grenades vs library) of one match from its stored timeline.</summary>
public record GetMatchAnalysisQuery(Guid MatchResultId) : IRequest<ErrorOr<MatchAnalysisDto>>;
