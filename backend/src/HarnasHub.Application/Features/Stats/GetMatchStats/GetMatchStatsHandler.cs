using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Stats.GetMatchStats;

/// <summary>Handles <see cref="GetMatchStatsQuery"/> by joining stats with the roster for display names.</summary>
public class GetMatchStatsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetMatchStatsQuery, ErrorOr<List<PlayerMatchStatDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<PlayerMatchStatDto>>> Handle(GetMatchStatsQuery request, CancellationToken cancellationToken)
	{
		return await (
			from stat in dbContext.PlayerMatchStats
			join user in dbContext.Users on stat.UserId equals user.Id
			where stat.MatchResultId == request.MatchResultId
			orderby stat.Rating descending
			select new PlayerMatchStatDto(
				stat.Id, user.Id, user.DisplayName, stat.Kills, stat.Deaths, stat.Assists, stat.Adr, stat.HeadshotPercentage, stat.Rating))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
