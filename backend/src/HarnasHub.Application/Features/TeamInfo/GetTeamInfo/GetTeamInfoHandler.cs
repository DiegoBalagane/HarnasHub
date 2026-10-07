using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.TeamInfo.GetTeamInfo;

/// <summary>Handles <see cref="GetTeamInfoQuery"/>, ordering by category then manual sort order.</summary>
public class GetTeamInfoHandler(IApplicationDbContext dbContext) : IRequestHandler<GetTeamInfoQuery, ErrorOr<List<TeamInfoEntryDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<TeamInfoEntryDto>>> Handle(GetTeamInfoQuery request, CancellationToken cancellationToken)
	{
		var entries = await dbContext.TeamInfoEntries
			.AsNoTracking()
			.OrderBy(e => e.Category)
			.ThenBy(e => e.SortOrder)
			.ThenBy(e => e.Title)
			.ToListAsync(cancellationToken);

		return entries.Select(TeamInfoEntryDto.From).ToList();
	}

	#endregion
}
