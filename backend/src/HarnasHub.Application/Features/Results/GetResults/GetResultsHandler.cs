using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.GetResults;

/// <summary>Handles <see cref="GetResultsQuery"/>.</summary>
public class GetResultsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetResultsQuery, ErrorOr<List<MatchResultDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<MatchResultDto>>> Handle(GetResultsQuery request, CancellationToken cancellationToken)
	{
		return await dbContext.MatchResults
			.GroupJoin(dbContext.Tournaments, m => m.TournamentId, t => t.Id, (m, tournaments) => new { m, tournaments })
			.SelectMany(x => x.tournaments.DefaultIfEmpty(), (x, tournament) => new { x.m, tournament })
			.GroupJoin(dbContext.Leagues, x => x.m.LeagueId, l => l.Id, (x, leagues) => new { x.m, x.tournament, leagues })
			.SelectMany(x => x.leagues.DefaultIfEmpty(), (x, league) => new { x.m, x.tournament, league })
			.OrderByDescending(x => x.m.PlayedAtUtc)
			.Select(x => new MatchResultDto(
				x.m.Id,
				x.m.Opponent,
				x.m.OurScore,
				x.m.OpponentScore,
				x.m.MapName,
				x.m.DemoUrl,
				x.m.Notes,
				x.m.PlayedAtUtc,
				x.m.Category,
				x.tournament == null ? null : x.tournament.Id,
				x.tournament == null ? null : x.tournament.Name,
				x.league == null ? null : x.league.Id,
				x.league == null ? null : x.league.Name,
				x.league == null ? null : x.league.Season,
				x.league == null ? null : x.league.Type))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
