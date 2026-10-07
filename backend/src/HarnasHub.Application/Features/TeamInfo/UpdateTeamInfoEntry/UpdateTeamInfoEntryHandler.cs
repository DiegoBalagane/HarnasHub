using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.TeamInfo.UpdateTeamInfoEntry;

/// <summary>Handles <see cref="UpdateTeamInfoEntryCommand"/>.</summary>
public class UpdateTeamInfoEntryHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateTeamInfoEntryCommand, ErrorOr<TeamInfoEntryDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TeamInfoEntryDto>> Handle(UpdateTeamInfoEntryCommand request, CancellationToken cancellationToken)
	{
		var entry = await dbContext.TeamInfoEntries.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
		if (entry is null)
		{
			return TeamInfoErrors.EntryNotFound;
		}

		var category = request.Category.Trim();
		if (!string.Equals(entry.Category, category, StringComparison.Ordinal))
		{
			var maxOrder = await dbContext.TeamInfoEntries
				.Where(e => e.Category == category)
				.Select(e => (int?)e.SortOrder)
				.MaxAsync(cancellationToken);
			entry.Category = category;
			entry.SortOrder = (maxOrder ?? -1) + 1;
		}

		entry.Title = request.Title.Trim();
		entry.Value = request.Value.Trim();
		entry.IsSecret = request.IsSecret;
		entry.UpdatedByUserId = currentUser.UserId;
		entry.UpdatedAtUtc = DateTime.UtcNow;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("team-info", cancellationToken);

		return TeamInfoEntryDto.From(entry);
	}

	#endregion
}
