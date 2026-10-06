using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.GamePlan.Shared;

/// <summary>Loads an event's game plan into its DTO; shared by the read and the write slice so both return the same shape.</summary>
public static class GamePlanReader
{
	#region Public Methods

	/// <summary>Builds the plan of <paramref name="eventId"/>, skipping items whose tactic or board was deleted since.</summary>
	public static async Task<EventGamePlanDto> ReadAsync(IApplicationDbContext dbContext, Guid eventId, CancellationToken cancellationToken)
	{
		var plan = await dbContext.EventGamePlans.FirstOrDefaultAsync(p => p.EventId == eventId, cancellationToken);

		var items = await dbContext.EventGamePlanItems
			.Where(i => i.EventId == eventId)
			.OrderBy(i => i.Order)
			.ToListAsync(cancellationToken);

		var tacticIds = items.Where(i => i.Kind == GamePlanItemKind.Tactic).Select(i => i.TargetId).ToList();
		var boardIds = items.Where(i => i.Kind == GamePlanItemKind.AnalysisBoard).Select(i => i.TargetId).ToList();

		var tactics = await dbContext.Tactics
			.Where(t => tacticIds.Contains(t.Id))
			.Select(t => new GamePlanTacticDto(t.Id, t.Name, t.MapName.ToString(), t.Side.ToString(), t.Economy.ToString()))
			.ToDictionaryAsync(t => t.Id, cancellationToken);

		var boards = await dbContext.AnalysisBoards
			.Where(b => boardIds.Contains(b.Id))
			.Select(b => new GamePlanBoardDto(b.Id, b.Title, b.MapName.ToString()))
			.ToDictionaryAsync(b => b.Id, cancellationToken);

		return new EventGamePlanDto(
			eventId,
			plan?.Notes,
			tacticIds.Where(tactics.ContainsKey).Select(id => tactics[id]).ToList(),
			boardIds.Where(boards.ContainsKey).Select(id => boards[id]).ToList(),
			plan?.UpdatedAtUtc);
	}

	#endregion
}
