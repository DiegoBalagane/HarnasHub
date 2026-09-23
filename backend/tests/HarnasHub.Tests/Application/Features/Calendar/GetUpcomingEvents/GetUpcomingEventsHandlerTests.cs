using HarnasHub.Application.Features.Calendar.GetUpcomingEvents;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.GetUpcomingEvents;

public class GetUpcomingEventsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_only_return_future_events_by_default()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Events.Add(PastEvent());
		var future = FutureEvent();
		dbContext.Events.Add(future);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetUpcomingEventsHandler(dbContext);

		var result = await handler.Handle(new GetUpcomingEventsQuery(), CancellationToken.None);

		Assert.False(result.IsError);
		var eventDto = Assert.Single(result.Value);
		Assert.Equal(future.Id, eventDto.Id);
	}

	[Fact]
	public async Task Should_return_every_event_most_recent_first_when_including_past()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var past = PastEvent();
		var future = FutureEvent();
		dbContext.Events.Add(past);
		dbContext.Events.Add(future);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetUpcomingEventsHandler(dbContext);

		var result = await handler.Handle(new GetUpcomingEventsQuery(IncludePast: true), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(2, result.Value.Count);
		Assert.Equal(future.Id, result.Value[0].Id);
		Assert.Equal(past.Id, result.Value[1].Id);
	}

	#endregion

	#region Private Methods

	private static Event PastEvent() => new()
	{
		Id = Guid.NewGuid(),
		Title = "Stary sparing",
		Type = EventType.Scrim,
		StartsAtUtc = DateTime.UtcNow.AddDays(-7),
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = DateTime.UtcNow
	};

	private static Event FutureEvent() => new()
	{
		Id = Guid.NewGuid(),
		Title = "Nowy sparing",
		Type = EventType.Scrim,
		StartsAtUtc = DateTime.UtcNow.AddDays(7),
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
