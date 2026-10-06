#region Usings

using ErrorOr;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMapAnalytics;

/// <summary>Aggregates the stored timelines of the last analysed matches on one map into the Playbook's map statistics.</summary>
public record GetMapAnalyticsQuery(MapName Map) : IRequest<ErrorOr<MapAnalyticsDto>>;
