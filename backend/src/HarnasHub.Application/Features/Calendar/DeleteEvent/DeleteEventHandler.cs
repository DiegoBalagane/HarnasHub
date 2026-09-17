using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Calendar.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Calendar.DeleteEvent;

/// <summary>Handles <see cref="DeleteEventCommand"/> — also clears any availability declarations for the event, since nothing else references them once it's gone.</summary>
public class DeleteEventHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteEventCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
	{
		var calendarEvent = await dbContext.Events
			.FirstOrDefaultAsync(e => e.Id == request.EventId, cancellationToken);

		if (calendarEvent is null)
		{
			return CalendarErrors.EventNotFound;
		}

		var declarations = await dbContext.Availabilities
			.Where(a => a.EventId == request.EventId)
			.ToListAsync(cancellationToken);
		dbContext.Availabilities.RemoveRange(declarations);

		dbContext.Events.Remove(calendarEvent);
		await dbContext.SaveChangesAsync(cancellationToken);

		await realtimeNotifier.NotifyAsync("calendar", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
