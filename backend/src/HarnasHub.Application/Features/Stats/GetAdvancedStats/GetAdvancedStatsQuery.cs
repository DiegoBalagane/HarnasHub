#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Stats.Shared;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Stats.GetAdvancedStats;

/// <summary>Aggregates the demo-derived statistics of our players across the newest analysed matches. <paramref name="Category"/>
/// and <paramref name="Map"/> narrow the matches, <paramref name="Last"/> keeps only the newest N of them (capped by
/// <see cref="GetAdvancedStatsHandler.MaxMatches"/>); null means no narrowing.</summary>
public record GetAdvancedStatsQuery(MatchCategory? Category, MapName? Map, int? Last) : IRequest<ErrorOr<AdvancedStatsDto>>;
