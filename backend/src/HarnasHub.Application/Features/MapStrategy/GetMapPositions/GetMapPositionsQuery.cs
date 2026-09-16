using ErrorOr;
using HarnasHub.Application.Features.MapStrategy.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.GetMapPositions;

/// <summary>Returns every player's starting spot for one map and side.</summary>
public record GetMapPositionsQuery(MapName MapName, MapSide Side) : IRequest<ErrorOr<List<MapPositionDto>>>;
