#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Tactics.GetMatchTacticMatches;

/// <summary>Matches every round of one of our matches against the Playbook tactics of its map and side.</summary>
public record GetMatchTacticMatchesQuery(Guid MatchResultId) : IRequest<ErrorOr<MatchTacticMatchesDto>>;
