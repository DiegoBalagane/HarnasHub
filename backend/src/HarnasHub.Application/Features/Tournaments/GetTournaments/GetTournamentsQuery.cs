using ErrorOr;
using HarnasHub.Application.Features.Tournaments.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tournaments.GetTournaments;

/// <summary>Returns every tournament, alphabetically.</summary>
public record GetTournamentsQuery : IRequest<ErrorOr<List<TournamentDto>>>;
