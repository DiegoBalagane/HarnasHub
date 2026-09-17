using HarnasHub.Application.Features.Availability.SetDayAvailability;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.SetDayAvailability;

public class SetDayAvailabilityHandlerTests
{
	#region Private Fields

	// Always "today" rather than a fixed literal, so these cases keep passing the handler's
	// not-in-the-past check regardless of when the suite runs.
	private static readonly DateOnly Day = DateOnly.FromDateTime(DateTime.UtcNow);
	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_insert_a_new_row_and_notify_when_nothing_was_declared_yet()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new SetDayAvailabilityHandler(dbContext, new TestCurrentUserService(_userId), notifier);

		var command = new SetDayAvailabilityCommand(
			Day,
			DayAvailabilityStatus.PartiallyAvailable,
			new TimeOnly(18, 0),
			new TimeOnly(21, 0),
			"Po pracy");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);

		var stored = await dbContext.PlayerAvailabilityDays.SingleAsync();
		Assert.Equal(_userId, stored.UserId);
		Assert.Equal(Day, stored.Date);
		Assert.Equal(DayAvailabilityStatus.PartiallyAvailable, stored.Status);
		Assert.Equal(new TimeOnly(18, 0), stored.AvailableFromLocal);
		Assert.Equal(new TimeOnly(21, 0), stored.AvailableToLocal);
		Assert.Equal("Po pracy", stored.Note);
		Assert.Equal(["availability-week", "dashboard"], notifier.Topics);
	}

	[Fact]
	public async Task Should_overwrite_the_existing_row_for_the_same_user_and_day()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
		{
			Id = Guid.NewGuid(),
			UserId = _userId,
			Date = Day,
			Status = DayAvailabilityStatus.PartiallyAvailable,
			AvailableFromLocal = new TimeOnly(18, 0),
			AvailableToLocal = new TimeOnly(21, 0),
			Note = "Po pracy",
			UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetDayAvailabilityHandler(
			dbContext,
			new TestCurrentUserService(_userId),
			new TestRealtimeNotifier());

		var command = new SetDayAvailabilityCommand(Day, DayAvailabilityStatus.Off, null, null, null);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);

		var stored = await dbContext.PlayerAvailabilityDays.SingleAsync();
		Assert.Equal(DayAvailabilityStatus.Off, stored.Status);
		Assert.Null(stored.AvailableFromLocal);
		Assert.Null(stored.AvailableToLocal);
		Assert.Null(stored.Note);
	}

	[Fact]
	public async Task Should_keep_other_users_declarations_untouched()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var otherUserId = Guid.NewGuid();
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
		{
			Id = Guid.NewGuid(),
			UserId = otherUserId,
			Date = Day,
			Status = DayAvailabilityStatus.Available,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetDayAvailabilityHandler(
			dbContext,
			new TestCurrentUserService(_userId),
			new TestRealtimeNotifier());

		await handler.Handle(
			new SetDayAvailabilityCommand(Day, DayAvailabilityStatus.Off, null, null, null),
			CancellationToken.None);

		var rows = await dbContext.PlayerAvailabilityDays.ToListAsync();
		Assert.Equal(2, rows.Count);
		Assert.Equal(DayAvailabilityStatus.Available, rows.Single(row => row.UserId == otherUserId).Status);
	}

	[Fact]
	public async Task Should_reject_a_past_date_for_a_regular_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetDayAvailabilityHandler(
			dbContext,
			new TestCurrentUserService(_userId, "Player"),
			new TestRealtimeNotifier());

		var pastDay = Day.AddDays(-1);
		var result = await handler.Handle(
			new SetDayAvailabilityCommand(pastDay, DayAvailabilityStatus.Off, null, null, null),
			CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Empty(dbContext.PlayerAvailabilityDays);
	}

	[Fact]
	public async Task Should_allow_a_coach_to_edit_a_past_date()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		// A coach keeps the Player access level — the coach tag alone has to unlock past-date edits.
		var handler = new SetDayAvailabilityHandler(
			dbContext,
			new TestCurrentUserService(_userId, "Player", isCoach: true),
			new TestRealtimeNotifier());

		var pastDay = Day.AddDays(-1);
		var result = await handler.Handle(
			new SetDayAvailabilityCommand(pastDay, DayAvailabilityStatus.Off, null, null, null),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(pastDay, (await dbContext.PlayerAvailabilityDays.SingleAsync()).Date);
	}

	[Fact]
	public async Task Should_allow_a_manager_to_edit_a_past_date()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetDayAvailabilityHandler(
			dbContext,
			new TestCurrentUserService(_userId, "Manager"),
			new TestRealtimeNotifier());

		var pastDay = Day.AddDays(-1);
		var result = await handler.Handle(
			new SetDayAvailabilityCommand(pastDay, DayAvailabilityStatus.Off, null, null, null),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(pastDay, (await dbContext.PlayerAvailabilityDays.SingleAsync()).Date);
	}

	#endregion
}
