using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TeamInfo.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.TeamInfo.CreateTeamInfoEntry;

/// <summary>Handles <see cref="CreateTeamInfoEntryCommand"/>, appending the entry to its category.</summary>
public class CreateTeamInfoEntryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<CreateTeamInfoEntryCommand, ErrorOr<TeamInfoEntryDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamInfoEntryDto>> Handle(CreateTeamInfoEntryCommand request, CancellationToken cancellationToken)
	{
		var category = request.Category.Trim();
		var maxOrder = await dbContext.TeamInfoEntries
			.Where(e => e.Category == category)
			.Select(e => (int?)e.SortOrder)
			.MaxAsync(cancellationToken);

		var entry = new TeamInfoEntry
		{
			Id = Guid.NewGuid(),
			Category = category,
			Title = request.Title.Trim(),
			Value = request.Value.Trim(),
			IsSecret = request.IsSecret,
			SortOrder = (maxOrder ?? -1) + 1,
			UpdatedByUserId = currentUser.UserId,
			UpdatedAtUtc = DateTime.UtcNow,
		};

		dbContext.TeamInfoEntries.Add(entry);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("team-info", cancellationToken);

		return TeamInfoEntryDto.From(entry);
	}

	#endregion
}
