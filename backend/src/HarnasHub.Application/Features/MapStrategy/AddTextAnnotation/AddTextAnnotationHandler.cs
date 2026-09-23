using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.AddTextAnnotation;

/// <summary>Handles <see cref="AddTextAnnotationCommand"/>.</summary>
public class AddTextAnnotationHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<AddTextAnnotationCommand, ErrorOr<MapTextAnnotationDto>>
{
	#region Public Methods

	public async Task<ErrorOr<MapTextAnnotationDto>> Handle(AddTextAnnotationCommand request, CancellationToken cancellationToken)
	{
		var annotation = new MapTextAnnotation
		{
			Id = Guid.NewGuid(),
			MapName = request.MapName,
			Side = request.Side,
			Text = request.Text,
			Color = request.Color,
			FontSizePx = request.FontSizePx,
			X = request.X,
			Y = request.Y,
			CreatedByUserId = currentUser.UserId,
			UpdatedAtUtc = DateTime.UtcNow
		};

		dbContext.MapTextAnnotations.Add(annotation);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("map-strategy", cancellationToken);

		return new MapTextAnnotationDto(annotation.Id, annotation.Text, annotation.Color, annotation.FontSizePx, annotation.X, annotation.Y);
	}

	#endregion
}
