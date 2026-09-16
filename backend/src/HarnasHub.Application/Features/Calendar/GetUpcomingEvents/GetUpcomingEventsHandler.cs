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
		var now = DateTime.UtcNow;

		return await dbContext.Events
			.Where(e => e.StartsAtUtc >= now)
			.OrderBy(e => e.StartsAtUtc)
			.Select(e => new EventDto(e.Id, e.Title, e.Type.ToString(), e.StartsAtUtc, e.Location, e.Notes))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
