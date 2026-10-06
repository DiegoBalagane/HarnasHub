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
	public async Task Should_delete_the_event_its_availability_declarations_veto_and_game_plan()
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
		dbContext.EventVetoSteps.Add(new EventVetoStep
		{
			Id = Guid.NewGuid(),
			EventId = calendarEvent.Id,
			Order = 1,
			Actor = VetoActor.Us,
			Action = VetoAction.Ban,
			MapName = MapName.Dust2
		});
		dbContext.EventGamePlans.Add(new EventGamePlan { Id = Guid.NewGuid(), EventId = calendarEvent.Id, Notes = "Plan" });
		dbContext.EventGamePlanItems.Add(new EventGamePlanItem
		{
			Id = Guid.NewGuid(),
			EventId = calendarEvent.Id,
			Kind = GamePlanItemKind.Tactic,
			TargetId = Guid.NewGuid(),
			Order = 1
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new DeleteEventHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteEventCommand(calendarEvent.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.Events.ToListAsync());
		Assert.Empty(await dbContext.Availabilities.ToListAsync());
		Assert.Empty(await dbContext.EventVetoSteps.ToListAsync());
		Assert.Empty(await dbContext.EventGamePlans.ToListAsync());
		Assert.Empty(await dbContext.EventGamePlanItems.ToListAsync());
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
