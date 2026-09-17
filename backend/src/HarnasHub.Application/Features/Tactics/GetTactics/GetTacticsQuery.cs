using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Tactics.GetTactics;

/// <summary>Returns saved tactics, optionally filtered by map, side and/or economy.</summary>
public record GetTacticsQuery(MapName? MapName, MapSide? Side, EconomyType? Economy) : IRequest<ErrorOr<List<TacticDto>>>;
