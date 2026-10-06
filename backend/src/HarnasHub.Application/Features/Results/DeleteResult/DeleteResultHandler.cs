using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Results.DeleteResult;

/// <summary>Handles <see cref="DeleteResultCommand"/> by removing the result and its stat lines — there's no FK
/// between <c>PlayerMatchStat.MatchResultId</c> and <c>MatchResult</c> (a loose link, like the rest of this app's
/// cross-entity references), so the stats (and the demo timeline row + its stored file) have to be deleted explicitly or they'd outlive the result.</summary>
public class DeleteResultHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier, IFileStorage fileStorage)
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

		var analysis = await dbContext.MatchDemoAnalyses.FirstOrDefaultAsync(a => a.MatchResultId == request.MatchResultId, cancellationToken);
		if (analysis is not null)
		{
			dbContext.MatchDemoAnalyses.Remove(analysis);
		}

		dbContext.MatchResults.Remove(result);
		await dbContext.SaveChangesAsync(cancellationToken);

		if (analysis is not null && fileStorage.IsConfigured)
		{
			try
			{
				await fileStorage.DeleteAsync(analysis.ObjectKey, CancellationToken.None);
			}
			catch (Exception)
			{
				// Best-effort — an orphaned timeline file is harmless and never read again.
			}
		}

		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("stats", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
