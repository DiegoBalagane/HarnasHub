using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.RemoveTextAnnotation;

/// <summary>Removes one text annotation from the map.</summary>
public record RemoveTextAnnotationCommand(Guid AnnotationId) : IRequest<ErrorOr<Success>>;
