using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Calendar.GetUpcomingEvents;

/// <summary>Handles <see cref="GetUpcomingEventsQuery"/>.</summary>
public class GetUpcomingEventsHandler(IApplicationDbContext dbContext)
	: IRequestHandler<GetUpcomingEventsQuery, ErrorOr<List<EventDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<EventDto>>> Handle(GetUpcomingEventsQuery request, CancellationToken cancellationToken)
	{
		if (request.IncludePast)
		{
			return await dbContext.Events
				.OrderByDescending(e => e.StartsAtUtc)
				.Select(EventMappings.Projection)
				.ToListAsync(cancellationToken);
		}

		var now = DateTime.UtcNow;

		return await dbContext.Events
			.Where(e => e.StartsAtUtc >= now)
			.OrderBy(e => e.StartsAtUtc)
			.Select(EventMappings.Projection)
			.ToListAsync(cancellationToken);
	}

	#endregion
}
