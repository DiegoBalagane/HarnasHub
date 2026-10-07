using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.TeamInfo.ReorderTeamInfoEntries;

/// <summary>Handles <see cref="ReorderTeamInfoEntriesCommand"/>; an unknown id fails the whole request so a stale client can't half-apply an order.</summary>
public class ReorderTeamInfoEntriesHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<ReorderTeamInfoEntriesCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(ReorderTeamInfoEntriesCommand request, CancellationToken cancellationToken)
	{
		var entries = await dbContext.TeamInfoEntries
			.Where(e => request.OrderedIds.Contains(e.Id))
			.ToDictionaryAsync(e => e.Id, cancellationToken);

		if (entries.Count != request.OrderedIds.Count)
		{
			return TeamInfoErrors.EntryNotFound;
		}

		for (var index = 0; index < request.OrderedIds.Count; index++)
		{
			entries[request.OrderedIds[index]].SortOrder = index;
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("team-info", cancellationToken);

		return Result.Success;
	}

	#endregion
}
