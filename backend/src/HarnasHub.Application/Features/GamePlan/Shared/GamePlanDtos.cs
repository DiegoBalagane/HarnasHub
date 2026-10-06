namespace HarnasHub.Application.Features.GamePlan.Shared;

/// <summary>A tactic attached to a game plan; enum-backed fields are their enum names.</summary>
public record GamePlanTacticDto(Guid Id, string Name, string MapName, string Side, string Economy);

/// <summary>An analysis board attached to a game plan.</summary>
public record GamePlanBoardDto(Guid Id, string Title, string MapName);

/// <summary>An event's game plan: the coach's notes plus the attached tactics and boards in the coach's order.
/// <paramref name="UpdatedAtUtc"/> is null when no plan was written yet.</summary>
public record EventGamePlanDto(
	Guid EventId,
	string? Notes,
	List<GamePlanTacticDto> Tactics,
	List<GamePlanBoardDto> Boards,
	DateTime? UpdatedAtUtc);
