using ErrorOr;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Stats.GetMatchStats;

/// <summary>Returns every player's stat line for one match.</summary>
public record GetMatchStatsQuery(Guid MatchResultId) : IRequest<ErrorOr<List<PlayerMatchStatDto>>>;
