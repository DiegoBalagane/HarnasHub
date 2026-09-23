using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tournaments.DeleteTournament;

/// <summary>Deletes a tournament. Matches already grouped under it keep their (now dangling) TournamentId,
/// same loose-link pattern as everywhere else in this app — they just stop resolving a tournament name.</summary>
public record DeleteTournamentCommand(Guid TournamentId) : IRequest<ErrorOr<Success>>;
