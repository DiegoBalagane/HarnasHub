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
			where stat.MatchResultId == request.MatchResultId
			// Left join: a departed player's stat line stays even after their account is deleted (see
			// DeleteTeamMemberHandler), so this must not silently drop rows whose user no longer exists.
			join user in dbContext.Users on stat.UserId equals user.Id into userGroup
			from user in userGroup.DefaultIfEmpty()
			orderby stat.Rating descending
			select new PlayerMatchStatDto(
				stat.Id,
				stat.UserId,
				user != null ? user.DisplayName : "Usunięty zawodnik",
				stat.Kills,
				stat.Deaths,
				stat.Assists,
				stat.Adr,
				stat.HeadshotPercentage,
				stat.Rating))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
