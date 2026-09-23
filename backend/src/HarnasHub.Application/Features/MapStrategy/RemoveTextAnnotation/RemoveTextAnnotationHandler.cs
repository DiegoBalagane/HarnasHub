using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapStrategy.RemoveTextAnnotation;

/// <summary>Handles <see cref="RemoveTextAnnotationCommand"/>.</summary>
public class RemoveTextAnnotationHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<RemoveTextAnnotationCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(RemoveTextAnnotationCommand request, CancellationToken cancellationToken)
	{
		var annotation = await dbContext.MapTextAnnotations
			.FirstOrDefaultAsync(a => a.Id == request.AnnotationId, cancellationToken);

		if (annotation is null)
		{
			return MapStrategyErrors.AnnotationNotFound;
		}

		dbContext.MapTextAnnotations.Remove(annotation);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return Result.Success;
	}

	#endregion
}
