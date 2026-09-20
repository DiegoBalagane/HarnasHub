using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Leagues.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Leagues.GetLeagues;

/// <summary>Handles <see cref="GetLeaguesQuery"/>.</summary>
public class GetLeaguesHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetLeaguesQuery, ErrorOr<List<LeagueDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<LeagueDto>>> Handle(GetLeaguesQuery request, CancellationToken cancellationToken)
	{
		return await dbContext.Leagues
			.OrderByDescending(l => l.CreatedAtUtc)
			.Select(l => new LeagueDto(l.Id, l.Name, l.Season, l.Type))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
