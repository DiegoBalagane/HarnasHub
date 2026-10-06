using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.DeleteEvent;
using HarnasHub.Application.Features.OpponentManagement.Shared;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.Results.DeleteResult;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.OpponentManagement.DeleteOpponent;

/// <summary>Handles <see cref="DeleteOpponentCommand"/>: results and events go through <see cref="DeleteResultCommand"/>/<see cref="DeleteEventCommand"/>
/// (so their timelines, stats and Discord notices behave as usual), scouting-only data is removed directly and its S3 timelines best-effort.</summary>
public class DeleteOpponentHandler(
	IApplicationDbContext dbContext,
	ISender sender,
	ICurrentUserService currentUser,
	IFileStorage fileStorage,
	IRealtimeNotifier realtimeNotifier,
	ILogger<DeleteOpponentHandler> logger) : IRequestHandler<DeleteOpponentCommand, ErrorOr<DeleteOpponentResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<DeleteOpponentResultDto>> Handle(DeleteOpponentCommand request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.Name);

		var matchIds = await dbContext.MatchResults
			.Where(m => m.Opponent.Trim().ToLower() == key).Select(m => m.Id).ToListAsync(cancellationToken);
		var eventIds = await dbContext.Events
			.Where(e => e.Opponent != null && e.Opponent.Trim().ToLower() == key).Select(e => e.Id).ToListAsync(cancellationToken);

		var notes = await dbContext.OpponentNotes.Where(n => n.OpponentName.Trim().ToLower() == key).ToListAsync(cancellationToken);
		var links = await dbContext.OpponentFaceitLinks.Where(l => l.OpponentKey == key).ToListAsync(cancellationToken);
		var snapshots = await dbContext.OpponentReportSnapshots.Where(s => s.OpponentKey == key).ToListAsync(cancellationToken);
		var analyses = await dbContext.OpponentDemoAnalyses.Where(a => a.OpponentKey == key).ToListAsync(cancellationToken);
		var hidden = await dbContext.HiddenOpponents.Where(h => h.OpponentKey == key).ToListAsync(cancellationToken);

		if (notes.Count + links.Count + snapshots.Count + analyses.Count + hidden.Count + matchIds.Count + eventIds.Count == 0)
		{
			return OpponentManagementErrors.NotFound;
		}

		var deletedMatches = 0;
		var deletedEvents = 0;
		if (request.IncludeHistory)
		{
			foreach (var id in matchIds)
			{
				if (!(await sender.Send(new DeleteResultCommand(id), cancellationToken)).IsError)
				{
					deletedMatches++;
				}
			}

			foreach (var id in eventIds)
			{
				if (!(await sender.Send(new DeleteEventCommand(id), cancellationToken)).IsError)
				{
					deletedEvents++;
				}
			}
		}

		// Results/events that stay would bring the name back into the list, so the opponent is hidden instead.
		var historyRemains = !request.IncludeHistory && (matchIds.Count > 0 || eventIds.Count > 0);

		dbContext.OpponentNotes.RemoveRange(notes);
		dbContext.OpponentFaceitLinks.RemoveRange(links);
		dbContext.OpponentReportSnapshots.RemoveRange(snapshots);
		dbContext.OpponentDemoAnalyses.RemoveRange(analyses);

		if (!historyRemains)
		{
			dbContext.HiddenOpponents.RemoveRange(hidden);
		}
		else if (hidden.Count == 0)
		{
			dbContext.HiddenOpponents.Add(new HiddenOpponent
			{
				Id = Guid.NewGuid(),
				OpponentKey = key,
				DisplayName = request.Name.Trim(),
				HiddenAtUtc = DateTime.UtcNow,
				HiddenByUserId = currentUser.UserId
			});
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await DeleteTimelinesAsync(analyses, cancellationToken);
		await realtimeNotifier.NotifyAsync("opponents", cancellationToken);

		return new DeleteOpponentResultDto(notes.Count, analyses.Count, deletedMatches, deletedEvents, historyRemains);
	}

	#endregion

	#region Private Methods

	/// <summary>Removes the S3 timelines of the deleted analyses; a failure is logged and never fails the delete (an orphaned object is harmless).</summary>
	private async Task DeleteTimelinesAsync(List<OpponentDemoAnalysis> analyses, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return;
		}

		foreach (var analysis in analyses)
		{
			try
			{
				await fileStorage.DeleteAsync(analysis.TimelineObjectKey, cancellationToken);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				logger.LogWarning(ex, "Nie udało się usunąć osi czasu demki przeciwnika (ObjectKey={ObjectKey})", analysis.TimelineObjectKey);
			}
		}
	}

	#endregion
}
