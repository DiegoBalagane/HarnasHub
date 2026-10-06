using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentManagement.Shared;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.OpponentManagement.RenameOpponent;

/// <summary>Handles <see cref="RenameOpponentCommand"/> in one <c>SaveChanges</c> (one transaction): rewrites the name on notes, results and events,
/// then moves FACEIT link, report snapshot, demo analyses and hidden flag to the target key — when the target already has its own link/snapshot, the target's is kept.</summary>
public class RenameOpponentHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<RenameOpponentCommand, ErrorOr<RenameOpponentResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<RenameOpponentResultDto>> Handle(RenameOpponentCommand request, CancellationToken cancellationToken)
	{
		var fromKey = OpponentNames.ToKey(request.From);
		var newName = request.To.Trim();
		var toKey = OpponentNames.ToKey(newName);

		var notes = await dbContext.OpponentNotes.Where(n => n.OpponentName.Trim().ToLower() == fromKey).ToListAsync(cancellationToken);
		var matches = await dbContext.MatchResults.Where(m => m.Opponent.Trim().ToLower() == fromKey).ToListAsync(cancellationToken);
		var events = await dbContext.Events.Where(e => e.Opponent != null && e.Opponent.Trim().ToLower() == fromKey).ToListAsync(cancellationToken);
		var fromLink = await dbContext.OpponentFaceitLinks.FirstOrDefaultAsync(l => l.OpponentKey == fromKey, cancellationToken);
		var fromSnapshot = await dbContext.OpponentReportSnapshots.FirstOrDefaultAsync(s => s.OpponentKey == fromKey, cancellationToken);
		var analyses = await dbContext.OpponentDemoAnalyses.Where(a => a.OpponentKey == fromKey).ToListAsync(cancellationToken);
		var fromHidden = await dbContext.HiddenOpponents.FirstOrDefaultAsync(h => h.OpponentKey == fromKey, cancellationToken);

		if (notes.Count + matches.Count + events.Count + analyses.Count == 0 && fromLink is null && fromSnapshot is null && fromHidden is null)
		{
			return OpponentManagementErrors.NotFound;
		}

		foreach (var note in notes)
		{
			note.OpponentName = newName;
		}

		foreach (var match in matches)
		{
			match.Opponent = newName;
		}

		foreach (var calendarEvent in events)
		{
			calendarEvent.Opponent = newName;
		}

		var faceitDataKept = false;
		if (fromKey == toKey)
		{
			// Same team, different spelling — only the stored display names change.
			if (fromLink is not null)
			{
				fromLink.DisplayName = newName;
			}

			if (fromHidden is not null)
			{
				fromHidden.DisplayName = newName;
			}
		}
		else
		{
			faceitDataKept = await MoveFaceitDataAsync(fromLink, fromSnapshot, newName, toKey, cancellationToken);

			foreach (var analysis in analyses)
			{
				analysis.OpponentKey = toKey;
			}

			await MoveHiddenFlagAsync(fromHidden, newName, toKey, cancellationToken);
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("opponents", cancellationToken);
		await realtimeNotifier.NotifyAsync("results", cancellationToken);
		await realtimeNotifier.NotifyAsync("calendar", cancellationToken);

		return new RenameOpponentResultDto(newName, notes.Count, matches.Count, events.Count, faceitDataKept);
	}

	#endregion

	#region Private Methods

	/// <summary>Re-keys the source link/snapshot onto the target unless the target has its own (then the source's is dropped); returns true when a FACEIT link was dropped.</summary>
	private async Task<bool> MoveFaceitDataAsync(
		OpponentFaceitLink? fromLink,
		OpponentReportSnapshot? fromSnapshot,
		string newName,
		string toKey,
		CancellationToken cancellationToken)
	{
		var dropped = false;

		if (fromLink is not null)
		{
			if (await dbContext.OpponentFaceitLinks.AnyAsync(l => l.OpponentKey == toKey, cancellationToken))
			{
				dbContext.OpponentFaceitLinks.Remove(fromLink);
				dropped = true;
			}
			else
			{
				fromLink.OpponentKey = toKey;
				fromLink.DisplayName = newName;
			}
		}

		if (fromSnapshot is not null)
		{
			if (await dbContext.OpponentReportSnapshots.AnyAsync(s => s.OpponentKey == toKey, cancellationToken))
			{
				dbContext.OpponentReportSnapshots.Remove(fromSnapshot);
			}
			else
			{
				fromSnapshot.OpponentKey = toKey;
			}
		}

		return dropped;
	}

	/// <summary>A hidden opponent stays hidden under its new name; when the target is already hidden the duplicate row just goes.</summary>
	private async Task MoveHiddenFlagAsync(HiddenOpponent? fromHidden, string newName, string toKey, CancellationToken cancellationToken)
	{
		if (fromHidden is null)
		{
			return;
		}

		if (await dbContext.HiddenOpponents.AnyAsync(h => h.OpponentKey == toKey, cancellationToken))
		{
			dbContext.HiddenOpponents.Remove(fromHidden);
		}
		else
		{
			fromHidden.OpponentKey = toKey;
			fromHidden.DisplayName = newName;
		}
	}

	#endregion
}
