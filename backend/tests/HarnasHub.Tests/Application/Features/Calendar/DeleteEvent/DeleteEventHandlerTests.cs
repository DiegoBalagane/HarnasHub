using HarnasHub.Application.Features.Calendar.DeleteEvent;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.DeleteEvent;

public class DeleteEventHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_event_and_its_availability_declarations()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var calendarEvent = new Event
		{
			Id = Guid.NewGuid(),
			Title = "Sparing",
			Type = EventType.Scrim,
			StartsAtUtc = DateTime.UtcNow,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Events.Add(calendarEvent);
		dbContext.Availabilities.Add(new HarnasHub.Core.Entities.Availability
		{
			Id = Guid.NewGuid(),
			EventId = calendarEvent.Id,
			UserId = Guid.NewGuid(),
			Status = AvailabilityStatus.Available,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new DeleteEventHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteEventCommand(calendarEvent.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.Events.ToListAsync());
		Assert.Empty(await dbContext.Availabilities.ToListAsync());
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_event()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteEventHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteEventCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Calendar.EventNotFound", result.FirstError.Code);
	}

	#endregion
}
