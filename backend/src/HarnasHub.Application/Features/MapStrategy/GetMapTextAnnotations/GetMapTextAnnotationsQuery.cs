using ErrorOr;
using HarnasHub.Application.Features.MapStrategy.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.GetMapTextAnnotations;

/// <summary>Returns every free-floating text annotation for one map and side.</summary>
public record GetMapTextAnnotationsQuery(MapName MapName, MapSide Side) : IRequest<ErrorOr<List<MapTextAnnotationDto>>>;
