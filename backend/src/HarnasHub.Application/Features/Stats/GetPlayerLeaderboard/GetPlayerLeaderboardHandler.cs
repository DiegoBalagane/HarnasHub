using ErrorOr;
using HarnasHub.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Stats.GetPlayerLeaderboard;

/// <summary>Handles <see cref="GetPlayerLeaderboardQuery"/> by averaging every roster player's stat lines,
/// optionally narrowed to one <c>MatchCategory</c>.</summary>
public class GetPlayerLeaderboardHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetPlayerLeaderboardQuery, ErrorOr<List<PlayerLeaderboardEntryDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<PlayerLeaderboardEntryDto>>> Handle(GetPlayerLeaderboardQuery request, CancellationToken cancellationToken)
	{
		var rows = dbContext.PlayerMatchStats
			.Where(stat => stat.UserId != null)
			.Join(dbContext.MatchResults, stat => stat.MatchResultId, match => match.Id, (stat, match) => new { stat, match });

		if (request.Category is { } category)
		{
			rows = rows.Where(x => x.match.Category == category);
		}

		var grouped = await rows
			.GroupBy(x => x.stat.UserId!.Value)
			.Select(g => new
			{
				UserId = g.Key,
				MatchesPlayed = g.Count(),
				AvgKills = g.Average(x => (double)x.stat.Kills),
				AvgDeaths = g.Average(x => (double)x.stat.Deaths),
				AvgAssists = g.Average(x => (double)x.stat.Assists),
				AvgAdr = g.Average(x => x.stat.Adr),
				AvgRating = g.Average(x => x.stat.Rating),
				AvgHeadshotPercentage = g.Average(x => x.stat.HeadshotPercentage),
				AvgKastPercentage = g.Average(x => x.stat.KastPercentage)
			})
			.ToListAsync(cancellationToken);

		var userIds = grouped.Select(g => g.UserId).ToList();
		var users = await dbContext.Users
			.Where(u => userIds.Contains(u.Id))
			.ToDictionaryAsync(u => u.Id, cancellationToken);

		return grouped
			.Select(g =>
			{
				users.TryGetValue(g.UserId, out var user);
				return new PlayerLeaderboardEntryDto(
					g.UserId,
					user?.DisplayName ?? "Usunięty zawodnik",
					user?.InGameNickname,
					g.MatchesPlayed,
					Math.Round(g.AvgKills, 1),
					Math.Round(g.AvgDeaths, 1),
					Math.Round(g.AvgAssists, 1),
					Math.Round(g.AvgAdr, 1),
					Math.Round(g.AvgRating, 2),
					Math.Round(g.AvgHeadshotPercentage, 1),
					g.AvgKastPercentage is { } kast ? Math.Round(kast, 1) : null);
			})
			.OrderByDescending(e => e.AvgRating)
			.ToList();
	}

	#endregion
}
