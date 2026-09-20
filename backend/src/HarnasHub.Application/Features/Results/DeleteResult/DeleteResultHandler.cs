using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.DeleteResult;

/// <summary>Handles <see cref="DeleteResultCommand"/> by removing the result and its stat lines — there's no FK
/// between <c>PlayerMatchStat.MatchResultId</c> and <c>MatchResult</c> (a loose link, like the rest of this app's
/// cross-entity references), so the stats have to be deleted explicitly or they'd outlive the result they belong to.</summary>
public class DeleteResultHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteResultCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteResultCommand request, CancellationToken cancellationToken)
	{
		var result = await dbContext.MatchResults.FirstOrDefaultAsync(r => r.Id == request.MatchResultId, cancellationToken);

		if (result is null)
		{
			return ResultErrors.MatchNotFound;
		}

		var stats = await dbContext.PlayerMatchStats
			.Where(s => s.MatchResultId == request.MatchResultId)
			.ToListAsync(cancellationToken);

		dbContext.PlayerMatchStats.RemoveRange(stats);
		dbContext.MatchResults.Remove(result);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("stats", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
