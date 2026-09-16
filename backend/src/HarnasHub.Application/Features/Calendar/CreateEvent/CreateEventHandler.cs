using ErrorOr;
using HarnasHub.Application.Abstractions;
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
			Location = request.Location,
			Notes = request.Notes,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Events.Add(calendarEvent);
		await dbContext.SaveChangesAsync(cancellationToken);

		var typeLabel = calendarEvent.Type switch
		{
			EventType.Training => "Trening",
			EventType.PickupGame => "Gra luźna",
			EventType.Match => "Mecz",
			EventType.Tournament => "Turniej",
			_ => calendarEvent.Type.ToString()
		};

		await discordNotifier.SendAsync(
			$"📅 Nowe wydarzenie: **{calendarEvent.Title}** ({typeLabel}) — {calendarEvent.StartsAtUtc:dd.MM HH:mm}",
			cancellationToken);
		await realtimeNotifier.NotifyAsync("calendar", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new EventDto(
			calendarEvent.Id,
			calendarEvent.Title,
			calendarEvent.Type.ToString(),
			calendarEvent.StartsAtUtc,
			calendarEvent.Location,
			calendarEvent.Notes);
	}

	#endregion
}
