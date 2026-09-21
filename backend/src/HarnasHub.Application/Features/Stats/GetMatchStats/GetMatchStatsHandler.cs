using System.Text.Json;
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
		var rows = await (
			from stat in dbContext.PlayerMatchStats
			where stat.MatchResultId == request.MatchResultId
			// Left join: a departed player's stat line stays even after their account is deleted (see
			// DeleteTeamMemberHandler), so this must not silently drop rows whose user no longer exists.
			join user in dbContext.Users on stat.UserId equals user.Id into userGroup
			from user in userGroup.DefaultIfEmpty()
			orderby stat.Rating descending
			select new { stat, user })
			.ToListAsync(cancellationToken);

		// DeathPositionsJson deserialization can't be translated to SQL, so it happens here, after the query
		// that needs the roster join has already run.
		return rows
			.Select(row => new PlayerMatchStatDto(
				row.stat.Id,
				row.stat.UserId,
				row.stat.UserId == null
					? (row.stat.DemoPlayerName ?? "Niepołączony gracz")
					: (row.user != null ? (row.user.InGameNickname ?? row.user.DisplayName) : "Usunięty zawodnik"),
				row.stat.Kills,
				row.stat.Deaths,
				row.stat.Assists,
				row.stat.Adr,
				row.stat.HeadshotPercentage,
				row.stat.Rating,
				row.stat.EntryKills,
				row.stat.EntryDeaths,
				row.stat.KastPercentage,
				row.stat.MultiKill2K,
				row.stat.MultiKill3K,
				row.stat.MultiKill4K,
				row.stat.MultiKill5K,
				row.stat.UtilityDamage,
				row.stat.FlashAssists,
				row.stat.DeathPositionsJson is null
					? []
					: JsonSerializer.Deserialize<List<DeathPositionDto>>(row.stat.DeathPositionsJson) ?? []))
			.ToList();
	}

	#endregion
}
