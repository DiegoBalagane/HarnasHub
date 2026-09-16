using ErrorOr;
using HarnasHub.Application.Features.MapStrategy.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;

/// <summary>Upserts a player's starting spot on one map side; coordinates are radar-relative fractions in [0,1].</summary>
public record SetPlayerPositionCommand(
	MapName MapName,
	MapSide Side,
	Guid UserId,
	string? Label,
	float X,
	float Y,
	string? Note) : IRequest<ErrorOr<MapPositionDto>>;
