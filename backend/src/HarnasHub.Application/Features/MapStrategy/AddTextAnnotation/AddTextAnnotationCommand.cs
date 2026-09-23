using ErrorOr;
using HarnasHub.Application.Features.MapStrategy.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.AddTextAnnotation;

/// <summary>Adds a free-floating text annotation to a map/side. Coach/Manager only — enforced at the endpoint.</summary>
public record AddTextAnnotationCommand(
	MapName MapName,
	MapSide Side,
	string Text,
	string Color,
	int FontSizePx,
	float X,
	float Y) : IRequest<ErrorOr<MapTextAnnotationDto>>;
