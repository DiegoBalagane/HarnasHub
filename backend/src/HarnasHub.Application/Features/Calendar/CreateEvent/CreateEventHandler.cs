using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Calendar.CreateEvent;

/// <summary>Handles <see cref="CreateEventCommand"/> by persisting the new event and notifying the team.</summary>
public class CreateEventHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IDiscordNotifier discordNotifier,
	IRealtimeNotifier realtimeNotifier) : IRequestHandler<CreateEventCommand, ErrorOr<EventDto>>
{
	#region Public Methods

	public async Task<ErrorOr<EventDto>> Handle(CreateEventCommand request, CancellationToken cancellationToken)
	{
		var calendarEvent = new Event
		{
			Id = Guid.NewGuid(),
			Title = request.Title,
			Type = request.Type,
			StartsAtUtc = request.StartsAtUtc,
			EndsAtUtc = request.EndsAtUtc,
			Location = request.Location,
			Url = request.Url,
			Notes = request.Notes,
			Opponent = EventMappings.NormalizeOpponent(request.Opponent),
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Events.Add(calendarEvent);
		await OpponentRevival.ReviveAsync(dbContext, EventMappings.NormalizeOpponent(request.Opponent), cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);

		await discordNotifier.SendAsync(MatchEventFormatter.ChannelFor(calendarEvent.Type), MatchEventFormatter.Created(calendarEvent), cancellationToken);
		await realtimeNotifier.NotifyAsync("calendar", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return EventMappings.ToDto(calendarEvent);
	}

	#endregion
}
