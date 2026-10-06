using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Calendar.UpdateEvent;

/// <summary>Handles <see cref="UpdateEventCommand"/>.</summary>
public class UpdateEventHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier, IDiscordNotifier discordNotifier)
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

		var before = new Event { Type = calendarEvent.Type, Title = calendarEvent.Title, StartsAtUtc = calendarEvent.StartsAtUtc, EndsAtUtc = calendarEvent.EndsAtUtc, Location = calendarEvent.Location, Opponent = calendarEvent.Opponent };

		calendarEvent.Title = request.Title;
		calendarEvent.Type = request.Type;
		calendarEvent.StartsAtUtc = request.StartsAtUtc;
		calendarEvent.EndsAtUtc = request.EndsAtUtc;
		calendarEvent.Location = request.Location;
		calendarEvent.Url = request.Url;
		calendarEvent.Notes = request.Notes;
		calendarEvent.Opponent = EventMappings.NormalizeOpponent(request.Opponent);
		await OpponentRevival.ReviveIfRenamedAsync(dbContext, before.Opponent, calendarEvent.Opponent, cancellationToken);

		await dbContext.SaveChangesAsync(cancellationToken);

		// Only match changes are announced (to the match schedule); notes/link-only edits stay silent.
		if ((before.Type == EventType.Match || calendarEvent.Type == EventType.Match) && MatchEventFormatter.IsSignificantChange(before, calendarEvent))
		{
			await discordNotifier.SendAsync(DiscordChannel.MatchSchedule, MatchEventFormatter.Updated(calendarEvent), cancellationToken);
		}

		await realtimeNotifier.NotifyAsync("calendar", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return EventMappings.ToDto(calendarEvent);
	}

	#endregion
}
