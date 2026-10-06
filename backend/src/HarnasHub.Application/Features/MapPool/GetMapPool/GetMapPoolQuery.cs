using ErrorOr;
using HarnasHub.Application.Features.MapPool.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.MapPool.GetMapPool;

/// <summary>Returns every map of the pool with its status and the team's record on it, optionally counting only one
/// <see cref="MatchCategory"/> (e.g. official league games without scrims).</summary>
public record GetMapPoolQuery(MatchCategory? Category = null) : IRequest<ErrorOr<List<MapPoolMapDto>>>;
