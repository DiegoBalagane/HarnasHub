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
			.Select(m => new { m.PlayedAtUtc, Won = m.OurScore > m.OpponentScore, Draw = m.OurScore == m.OpponentScore })
			.ToListAsync(cancellationToken);

		var points = new List<TeamTrendPointDto>(matches.Count);
		var wins = 0;
		var losses = 0;
		var draws = 0;

		// A draw (e.g. a 12:12 scrim) is its own outcome — counting it as a loss made the trend look worse than reality.
		foreach (var match in matches)
		{
			if (match.Won)
			{
				wins++;
			}
			else if (match.Draw)
			{
				draws++;
			}
			else
			{
				losses++;
			}

			var winRate = (double)wins / (wins + losses + draws) * 100;
			points.Add(new TeamTrendPointDto(match.PlayedAtUtc, match.Won, wins, losses, winRate, match.Draw, draws));
		}

		return points;
	}

	#endregion
}
