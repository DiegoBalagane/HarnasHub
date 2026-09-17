using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Tactics.Shared;

/// <summary>One saved tactic, without its radar points — used for the browsable list.</summary>
public record TacticDto(
	Guid Id,
	MapName MapName,
	MapSide Side,
	string Name,
	string Economy,
	string? Note,
	Guid CreatedByUserId,
	int PointCount);

/// <summary>One numbered radar point belonging to a tactic.</summary>
public record TacticPointDto(
	Guid Id,
	int Order,
	float X,
	float Y,
	string? Description,
	Guid? NadeEntryId);

/// <summary>A tactic together with its full radar layout, used by the editor.</summary>
public record TacticDetailDto(
	Guid Id,
	MapName MapName,
	MapSide Side,
	string Name,
	string Economy,
	string? Note,
	Guid CreatedByUserId,
	List<TacticPointDto> Points);

/// <summary>One radar point as submitted by the editor when saving a tactic's layout.</summary>
public record TacticPointInput(float X, float Y, string? Description, Guid? NadeEntryId);
