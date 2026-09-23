using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Leagues.DeleteLeague;

/// <summary>Deletes a league season. Matches already grouped under it keep their (now dangling) LeagueId,
/// same loose-link pattern as everywhere else in this app — they just stop resolving a league name.</summary>
public record DeleteLeagueCommand(Guid LeagueId) : IRequest<ErrorOr<Success>>;
