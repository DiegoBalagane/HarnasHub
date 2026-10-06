#region Usings

using ErrorOr;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMatchTimeline;

/// <summary>Loads a match's round-by-round timeline (from its stored demo analysis) for the match page.</summary>
public record GetMatchTimelineQuery(Guid MatchResultId) : IRequest<ErrorOr<MatchTimelineDto>>;
