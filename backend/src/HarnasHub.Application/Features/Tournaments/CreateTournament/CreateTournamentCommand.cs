using ErrorOr;
using HarnasHub.Application.Features.Tournaments.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tournaments.CreateTournament;

/// <summary>Creates a tournament to group match results under. Coach/Manager only — enforced at the endpoint.</summary>
public record CreateTournamentCommand(string Name) : IRequest<ErrorOr<TournamentDto>>;
