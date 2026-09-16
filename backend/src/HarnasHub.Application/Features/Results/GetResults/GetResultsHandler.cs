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
			.OrderByDescending(m => m.PlayedAtUtc)
			.Select(m => new MatchResultDto(
				m.Id, m.Opponent, m.OurScore, m.OpponentScore, m.MapName, m.DemoUrl, m.Notes, m.PlayedAtUtc))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
