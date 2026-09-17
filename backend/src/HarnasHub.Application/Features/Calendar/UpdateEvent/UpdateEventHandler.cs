using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Calendar.UpdateEvent;

/// <summary>Handles <see cref="UpdateEventCommand"/>.</summary>
public class UpdateEventHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateEventCommand, ErrorOr<EventDto>>
{
	#region Public Methods

	public async Task<ErrorOr<EventDto>> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
	{
		var calendarEvent = await dbContext.Events
			.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

		if (calendarEvent is null)
		{
			return CalendarErrors.EventNotFound;
		}

		calendarEvent.Title = request.Title;
		calendarEvent.Type = request.Type;
		calendarEvent.StartsAtUtc = request.StartsAtUtc;
		calendarEvent.Location = request.Location;
		calendarEvent.Url = request.Url;
		calendarEvent.Notes = request.Notes;

		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("calendar", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new EventDto(
			calendarEvent.Id,
			calendarEvent.Title,
			calendarEvent.Type.ToString(),
			calendarEvent.StartsAtUtc,
			calendarEvent.Location,
			calendarEvent.Url,
			calendarEvent.Notes);
	}

	#endregion
}
