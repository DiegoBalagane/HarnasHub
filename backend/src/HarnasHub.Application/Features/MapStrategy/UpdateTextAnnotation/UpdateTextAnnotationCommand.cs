using ErrorOr;
using HarnasHub.Application.Features.MapStrategy.Shared;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.UpdateTextAnnotation;

/// <summary>Updates a text annotation's content, style, and/or position (a drag only changes X/Y, the editor panel
/// only changes Text/Color/FontSizePx — both go through this one command, resending the fields that didn't change).</summary>
public record UpdateTextAnnotationCommand(
	Guid AnnotationId,
	string Text,
	string Color,
	int FontSizePx,
	float X,
	float Y) : IRequest<ErrorOr<MapTextAnnotationDto>>;
