using HarnasHub.Application.Features.Calendar.CreateEvent;
using HarnasHub.Application.Features.Calendar.DeleteEvent;
using HarnasHub.Application.Features.Calendar.UpdateEvent;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar;

public class EventNotificationRoutingTests
{
	#region Private Fields

	private static readonly DateTime Start = new(2026, 9, 20, 18, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Theory]
	[InlineData(EventType.Match, DiscordChannel.MatchSchedule)]
	[InlineData(EventType.Training, DiscordChannel.Announcements)]
	public async Task Create_routes_the_announcement_by_event_type(EventType type, DiscordChannel expected)
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var discord = new TestDiscordNotifier();
		var handler = new CreateEventHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), discord, new TestRealtimeNotifier());

		await handler.Handle(new CreateEventCommand("Wydarzenie", type, Start, null, null, null, null), CancellationToken.None);

		Assert.Equal(expected, Assert.Single(discord.Sent).Channel);
	}

	[Fact]
	public async Task Update_of_a_match_time_posts_to_the_match_schedule()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var calendarEvent = await SeedAsync(dbContext, EventType.Match);
		var discord = new TestDiscordNotifier();

		await new UpdateEventHandler(dbContext, new TestRealtimeNotifier(), discord).Handle(
			new UpdateEventCommand(calendarEvent.Id, "Mecz", EventType.Match, Start.AddHours(1), null, null, null, null), CancellationToken.None);

		Assert.Equal(DiscordChannel.MatchSchedule, Assert.Single(discord.Sent).Channel);
	}

	[Fact]
	public async Task Update_that_only_changes_notes_stays_silent()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var calendarEvent = await SeedAsync(dbContext, EventType.Match);
		var discord = new TestDiscordNotifier();

		await new UpdateEventHandler(dbContext, new TestRealtimeNotifier(), discord).Handle(
			new UpdateEventCommand(calendarEvent.Id, "Mecz", EventType.Match, Start, null, null, null, "notatka"), CancellationToken.None);

		Assert.Empty(discord.Sent);
	}

	[Fact]
	public async Task Update_of_a_non_match_event_stays_silent()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var calendarEvent = await SeedAsync(dbContext, EventType.Training);
		var discord = new TestDiscordNotifier();

		await new UpdateEventHandler(dbContext, new TestRealtimeNotifier(), discord).Handle(
			new UpdateEventCommand(calendarEvent.Id, "Mecz", EventType.Training, Start.AddHours(1), null, null, null, null), CancellationToken.None);

		Assert.Empty(discord.Sent);
	}

	[Theory]
	[InlineData(EventType.Match, 1)]
	[InlineData(EventType.Training, 0)]
	public async Task Delete_announces_only_matches(EventType type, int expectedMessages)
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var calendarEvent = await SeedAsync(dbContext, type);
		var discord = new TestDiscordNotifier();

		await new DeleteEventHandler(dbContext, new TestRealtimeNotifier(), discord).Handle(new DeleteEventCommand(calendarEvent.Id), CancellationToken.None);

		Assert.Equal(expectedMessages, discord.Sent.Count(m => m.Channel == DiscordChannel.MatchSchedule));
		Assert.DoesNotContain(discord.Sent, m => m.Channel != DiscordChannel.MatchSchedule);
	}

	#endregion

	#region Private Methods

	private static async Task<Event> SeedAsync(TestApplicationDbContext dbContext, EventType type)
	{
		var calendarEvent = new Event
		{
			Id = Guid.NewGuid(),
			Title = "Mecz",
			Type = type,
			StartsAtUtc = Start,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Events.Add(calendarEvent);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		return calendarEvent;
	}

	#endregion
}
