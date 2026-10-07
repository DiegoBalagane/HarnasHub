using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.TeamInfo.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.TeamInfo.DeleteTeamInfoEntry;

/// <summary>Handles <see cref="DeleteTeamInfoEntryCommand"/>.</summary>
public class DeleteTeamInfoEntryHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteTeamInfoEntryCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteTeamInfoEntryCommand request, CancellationToken cancellationToken)
	{
		var entry = await dbContext.TeamInfoEntries.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
		if (entry is null)
		{
			return TeamInfoErrors.EntryNotFound;
		}

		dbContext.TeamInfoEntries.Remove(entry);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("team-info", cancellationToken);

		return Result.Success;
	}

	#endregion
}
