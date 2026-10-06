using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.GamePlan.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.GamePlan.SetEventGamePlan;

/// <summary>Handles <see cref="SetEventGamePlanCommand"/>: upserts the notes and replaces every attached item in one save.</summary>
public class SetEventGamePlanHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SetEventGamePlanCommand, ErrorOr<EventGamePlanDto>>
{
	#region Public Methods

	public async Task<ErrorOr<EventGamePlanDto>> Handle(SetEventGamePlanCommand request, CancellationToken cancellationToken)
	{
		if (!await dbContext.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken))
		{
			return GamePlanErrors.EventNotFound;
		}

		var knownTactics = await dbContext.Tactics.CountAsync(t => request.TacticIds.Contains(t.Id), cancellationToken);
		var knownBoards = await dbContext.AnalysisBoards.CountAsync(b => request.BoardIds.Contains(b.Id), cancellationToken);
		if (knownTactics != request.TacticIds.Count || knownBoards != request.BoardIds.Count)
		{
			return GamePlanErrors.UnknownItems;
		}

		var plan = await dbContext.EventGamePlans.FirstOrDefaultAsync(p => p.EventId == request.EventId, cancellationToken);
		if (plan is null)
		{
			plan = new EventGamePlan { Id = Guid.NewGuid(), EventId = request.EventId };
			dbContext.EventGamePlans.Add(plan);
		}

		plan.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
		plan.UpdatedByUserId = currentUser.UserId;
		plan.UpdatedAtUtc = DateTime.UtcNow;

		dbContext.EventGamePlanItems.RemoveRange(
			await dbContext.EventGamePlanItems.Where(i => i.EventId == request.EventId).ToListAsync(cancellationToken));
		dbContext.EventGamePlanItems.AddRange(
			BuildItems(request.EventId, GamePlanItemKind.Tactic, request.TacticIds)
				.Concat(BuildItems(request.EventId, GamePlanItemKind.AnalysisBoard, request.BoardIds)));

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("game-plan", cancellationToken);

		return await GamePlanReader.ReadAsync(dbContext, request.EventId, cancellationToken);
	}

	#endregion

	#region Private Methods

	/// <summary>One item per id, numbered in list order.</summary>
	private static IEnumerable<EventGamePlanItem> BuildItems(Guid eventId, GamePlanItemKind kind, List<Guid> targetIds) =>
		targetIds.Select((targetId, index) => new EventGamePlanItem
		{
			Id = Guid.NewGuid(),
			EventId = eventId,
			Kind = kind,
			TargetId = targetId,
			Order = index + 1
		});

	#endregion
}
