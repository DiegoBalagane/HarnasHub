using ErrorOr;
using HarnasHub.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Stats.GetTeamTrend;

/// <summary>Handles <see cref="GetTeamTrendQuery"/> by folding match results into a running win-rate series.</summary>
public class GetTeamTrendHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetTeamTrendQuery, ErrorOr<List<TeamTrendPointDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<TeamTrendPointDto>>> Handle(GetTeamTrendQuery request, CancellationToken cancellationToken)
	{
		var matches = await dbContext.MatchResults
			.OrderBy(m => m.PlayedAtUtc)
			.Select(m => new { m.PlayedAtUtc, Won = m.OurScore > m.OpponentScore })
			.ToListAsync(cancellationToken);

		var points = new List<TeamTrendPointDto>(matches.Count);
		var wins = 0;
		var losses = 0;

		foreach (var match in matches)
		{
			if (match.Won)
			{
				wins++;
			}
			else
			{
				losses++;
			}

			var winRate = (double)wins / (wins + losses) * 100;
			points.Add(new TeamTrendPointDto(match.PlayedAtUtc, match.Won, wins, losses, winRate));
		}

		return points;
	}

	#endregion
}
