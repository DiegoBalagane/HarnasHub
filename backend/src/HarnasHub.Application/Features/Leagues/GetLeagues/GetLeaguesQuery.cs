using ErrorOr;
using HarnasHub.Application.Features.Leagues.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Leagues.GetLeagues;

/// <summary>Returns every league season, most recently created first.</summary>
public record GetLeaguesQuery : IRequest<ErrorOr<List<LeagueDto>>>;
