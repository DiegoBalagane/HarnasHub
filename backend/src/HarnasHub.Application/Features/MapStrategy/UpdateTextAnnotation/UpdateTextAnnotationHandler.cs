using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapStrategy.UpdateTextAnnotation;

/// <summary>Handles <see cref="UpdateTextAnnotationCommand"/>.</summary>
public class UpdateTextAnnotationHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateTextAnnotationCommand, ErrorOr<MapTextAnnotationDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MapTextAnnotationDto>> Handle(UpdateTextAnnotationCommand request, CancellationToken cancellationToken)
	{
		var annotation = await dbContext.MapTextAnnotations
			.FirstOrDefaultAsync(a => a.Id == request.AnnotationId, cancellationToken);

		if (annotation is null)
		{
			return MapStrategyErrors.AnnotationNotFound;
		}

		annotation.Text = request.Text;
		annotation.Color = request.Color;
		annotation.FontSizePx = request.FontSizePx;
		annotation.X = request.X;
		annotation.Y = request.Y;
		annotation.UpdatedAtUtc = DateTime.UtcNow;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return new MapTextAnnotationDto(annotation.Id, annotation.Text, annotation.Color, annotation.FontSizePx, annotation.X, annotation.Y);
	}

	#endregion
}
