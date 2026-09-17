using HarnasHub.Application.Features.Availability.GetWeekAvailability;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.GetWeekAvailability;

public class GetWeekAvailabilityHandlerTests
{
	#region Private Fields

	private static readonly DateOnly WeekStart = new(2026, 9, 14);

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_return_seven_days_per_member_with_not_set_as_the_default()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddUser(dbContext, Guid.NewGuid(), "Zenek");
		AddUser(dbContext, Guid.NewGuid(), "Antek");
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["Antek", "Zenek"], result.Value.Members.Select(member => member.DisplayName));

		var days = result.Value.Members[0].Days;
		Assert.Equal(7, days.Count);
		Assert.Equal(WeekStart, days[0].Date);
		Assert.Equal(WeekStart.AddDays(6), days[6].Date);
		Assert.All(days, day => Assert.Equal("NotSet", day.Status));
		Assert.All(days, day => Assert.False(day.IsVacation));
	}

	[Fact]
	public async Task Should_return_the_declared_status_with_its_hours()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Date = WeekStart.AddDays(2),
			Status = DayAvailabilityStatus.PartiallyAvailable,
			AvailableFromLocal = new TimeOnly(18, 0),
			AvailableToLocal = new TimeOnly(21, 0),
			Note = "Po pracy",
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		var day = result.Value.Members.Single().Days[2];
		Assert.Equal(nameof(DayAvailabilityStatus.PartiallyAvailable), day.Status);
		Assert.Equal(new TimeOnly(18, 0), day.From);
		Assert.Equal(new TimeOnly(21, 0), day.To);
		Assert.Equal("Po pracy", day.Note);
		Assert.False(day.IsVacation);
	}

	[Fact]
	public async Task Should_override_declared_status_with_off_inside_a_vacation_range()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Date = WeekStart.AddDays(1),
			Status = DayAvailabilityStatus.Available,
			UpdatedAtUtc = DateTime.UtcNow
		});
		dbContext.Vacations.Add(new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			StartDate = WeekStart.AddDays(1),
			EndDate = WeekStart.AddDays(3),
			Reason = "Urlop",
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		var days = result.Value.Members.Single().Days;
		Assert.Equal("NotSet", days[0].Status);

		foreach (var day in days.Skip(1).Take(3))
		{
			Assert.Equal(nameof(DayAvailabilityStatus.Off), day.Status);
			Assert.True(day.IsVacation);
			Assert.Equal("Urlop", day.Note);
			Assert.Null(day.From);
		}

		Assert.Equal("NotSet", days[4].Status);
		Assert.False(days[4].IsVacation);
	}

	[Fact]
	public async Task Should_not_apply_another_members_vacation()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		AddUser(dbContext, Guid.NewGuid(), "Antek");
		dbContext.Vacations.Add(new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			StartDate = WeekStart,
			EndDate = WeekStart.AddDays(6),
			Reason = null,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		var antek = result.Value.Members.Single(member => member.DisplayName == "Antek");
		var zenek = result.Value.Members.Single(member => member.DisplayName == "Zenek");

		Assert.All(antek.Days, day => Assert.False(day.IsVacation));
		Assert.All(zenek.Days, day => Assert.True(day.IsVacation));
	}

	[Fact]
	public async Task Should_exclude_stand_ins_and_sort_main_before_bench()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var mainUser = CreateUserWithSlot("ZZZMain", RosterSlot.Main);
		var benchUser = CreateUserWithSlot("AAABench", RosterSlot.Bench);
		var standInUser = CreateUserWithSlot("StandIn", RosterSlot.StandIn);
		dbContext.Users.AddRange(mainUser, benchUser, standInUser);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		Assert.Equal(
			["ZZZMain", "AAABench"],
			result.Value.Members.Select(member => member.DisplayName));
		Assert.Equal(nameof(RosterSlot.Main), result.Value.Members[0].RosterSlot);
		Assert.Equal(nameof(RosterSlot.Bench), result.Value.Members[1].RosterSlot);
	}

	[Fact]
	public async Task Should_exclude_guests_and_unassigned_non_coaches()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var mainUser = CreateUserWithSlot("WSkładzie", RosterSlot.Main);
		var guestUser = CreateUserWithSlot("Gość", RosterSlot.Main, AccessLevel.Guest);
		var unassignedUser = CreateUserWithSlot("Pozostali", null);
		dbContext.Users.AddRange(mainUser, guestUser, unassignedUser);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		Assert.Equal(["WSkładzie"], result.Value.Members.Select(member => member.DisplayName));
	}

	[Fact]
	public async Task Should_include_a_coach_without_a_roster_slot_and_sort_them_last()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var mainUser = CreateUserWithSlot("ZZZMain", RosterSlot.Main);
		var benchUser = CreateUserWithSlot("AAABench", RosterSlot.Bench);
		var coach = CreateUserWithSlot("AAATrener", null, isCoach: true);
		dbContext.Users.AddRange(mainUser, benchUser, coach);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		// Alphabetically first, yet the coach still lands at the bottom in their own section.
		Assert.Equal(
			["ZZZMain", "AAABench", "AAATrener"],
			result.Value.Members.Select(member => member.DisplayName));
		Assert.True(result.Value.Members[2].IsCoach);
		Assert.Null(result.Value.Members[2].RosterSlot);
		Assert.All(result.Value.Members.Take(2), member => Assert.False(member.IsCoach));
	}

	[Fact]
	public async Task Should_sort_a_coach_last_even_when_they_hold_a_main_roster_slot()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var mainUser = CreateUserWithSlot("ZZZMain", RosterSlot.Main);
		var playingCoach = CreateUserWithSlot("AAAGrającyTrener", RosterSlot.Main, AccessLevel.Manager, isCoach: true);
		dbContext.Users.AddRange(mainUser, playingCoach);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		Assert.Equal(
			["ZZZMain", "AAAGrającyTrener"],
			result.Value.Members.Select(member => member.DisplayName));
	}

	[Fact]
	public async Task Should_expose_the_members_own_nickname_alongside_their_discord_name()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString(),
			DisplayName = "DiscordowaNazwa",
			InGameNickname = "Zenus",
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.Main,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext);

		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		var member = result.Value.Members.Single();
		Assert.Equal("DiscordowaNazwa", member.DisplayName);
		Assert.Equal("Zenus", member.InGameNickname);
	}

	#endregion

	#region Private Methods

	// Members need a roster slot to show up in the calendar at all, so the shared helper hands out Main by default.
	private static void AddUser(TestApplicationDbContext dbContext, Guid userId, string displayName) =>
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString(),
			DisplayName = displayName,
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.Main,
			CreatedAtUtc = DateTime.UtcNow
		});

	private static User CreateUserWithSlot(
		string displayName,
		RosterSlot? rosterSlot,
		AccessLevel accessLevel = AccessLevel.Player,
		bool isCoach = false)
	{
		var id = Guid.NewGuid();
		return new User
		{
			Id = id,
			DiscordId = id.ToString(),
			DisplayName = displayName,
			AccessLevel = accessLevel,
			IsCoach = isCoach,
			RosterSlot = rosterSlot,
			CreatedAtUtc = DateTime.UtcNow
		};
	}

	#endregion
}
