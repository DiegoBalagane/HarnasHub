using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapStrategy.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.MapStrategy.GetMapTextAnnotations;

/// <summary>Handles <see cref="GetMapTextAnnotationsQuery"/>.</summary>
public class GetMapTextAnnotationsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetMapTextAnnotationsQuery, ErrorOr<List<MapTextAnnotationDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<MapTextAnnotationDto>>> Handle(GetMapTextAnnotationsQuery request, CancellationToken cancellationToken)
	{
		return await dbContext.MapTextAnnotations
			.Where(a => a.MapName == request.MapName && a.Side == request.Side)
			.OrderBy(a => a.UpdatedAtUtc)
			.Select(a => new MapTextAnnotationDto(a.Id, a.Text, a.Color, a.FontSizePx, a.X, a.Y))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
