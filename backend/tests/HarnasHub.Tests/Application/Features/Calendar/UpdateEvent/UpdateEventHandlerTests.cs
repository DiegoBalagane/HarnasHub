using HarnasHub.Application.Features.Calendar.UpdateEvent;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.UpdateEvent;

public class UpdateEventHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_event_in_place()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var calendarEvent = new Event
		{
			Id = Guid.NewGuid(),
			Title = "Sparing",
			Type = EventType.Scrim,
			StartsAtUtc = new DateTime(2026, 9, 20, 18, 0, 0, DateTimeKind.Utc),
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Events.Add(calendarEvent);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateEventHandler(dbContext, new TestRealtimeNotifier());
		var newStart = new DateTime(2026, 9, 20, 19, 0, 0, DateTimeKind.Utc);

		var command = new UpdateEventCommand(
			calendarEvent.Id,
			"Sparing (poprawiona godzina)",
			EventType.Scrim,
			newStart,
			null,
			"Warszawa",
			"https://pracc.com/matches/3338510",
			"Notatka");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.Events.SingleAsync();
		Assert.Equal("Sparing (poprawiona godzina)", stored.Title);
		Assert.Equal(newStart, stored.StartsAtUtc);
		Assert.Equal("Warszawa", stored.Location);
		Assert.Equal("https://pracc.com/matches/3338510", stored.Url);
		Assert.Equal("Notatka", stored.Notes);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_event()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateEventHandler(dbContext, new TestRealtimeNotifier());

		var command = new UpdateEventCommand(
			Guid.NewGuid(),
			"Sparing",
			EventType.Scrim,
			DateTime.UtcNow,
			null,
			null,
			null,
			null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Calendar.EventNotFound", result.FirstError.Code);
	}

	#endregion
}
