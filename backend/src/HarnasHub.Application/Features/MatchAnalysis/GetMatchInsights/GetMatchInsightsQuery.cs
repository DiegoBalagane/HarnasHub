#region Usings

using ErrorOr;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMatchInsights;

/// <summary>Produces the automatic, rule-based insights for one match from its stored timeline.</summary>
public record GetMatchInsightsQuery(Guid MatchResultId) : IRequest<ErrorOr<IReadOnlyList<MatchInsightDto>>>;
