using ErrorOr;
using HarnasHub.Application.Features.Leagues.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Leagues.CreateLeague;

/// <summary>Creates a league season to group match results under. Coach/Manager only — enforced at the endpoint.</summary>
public record CreateLeagueCommand(string Name, string Season, LeagueType Type) : IRequest<ErrorOr<LeagueDto>>;
