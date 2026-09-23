using ErrorOr;
using HarnasHub.Application.Features.Dashboard.GetDashboardSummary;
using HarnasHub.Application.Features.Dashboard.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Dashboard.GetDashboardSummary;

public class GetDashboardSummaryHandlerTests
{
	#region Private Fields

	private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);
	private static readonly DateOnly Tomorrow = Today.AddDays(1);

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_return_today_and_tomorrow_with_not_set_as_the_default()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, userId);

		Assert.False(result.IsError);
		Assert.Equal(Today, result.Value.Today.Date);
		Assert.Equal(Tomorrow, result.Value.Tomorrow.Date);
		Assert.Equal("NotSet", result.Value.Today.Members.Single().Status);
		Assert.Equal("NotSet", result.Value.Tomorrow.Members.Single().Status);
		Assert.Null(result.Value.Today.Event);
		Assert.Null(result.Value.Tomorrow.Event);
	}

	[Fact]
	public async Task Should_return_the_declared_status_with_its_hours_for_today()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Date = Today,
			Status = DayAvailabilityStatus.PartiallyAvailable,
			AvailableFromLocal = new TimeOnly(18, 0),
			AvailableToLocal = new TimeOnly(21, 0),
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, userId);

		var member = result.Value.Today.Members.Single();
		Assert.Equal(nameof(DayAvailabilityStatus.PartiallyAvailable), member.Status);
		Assert.Equal(new TimeOnly(18, 0), member.From);
		Assert.Equal(new TimeOnly(21, 0), member.To);
		Assert.False(member.IsVacation);
		Assert.Equal("NotSet", result.Value.Tomorrow.Members.Single().Status);
	}

	[Fact]
	public async Task Should_override_tomorrow_with_off_when_a_vacation_covers_it()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			Date = Tomorrow,
			Status = DayAvailabilityStatus.Available,
			UpdatedAtUtc = DateTime.UtcNow
		});
		dbContext.Vacations.Add(new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			StartDate = Tomorrow,
			EndDate = Tomorrow.AddDays(3),
			Reason = "Urlop",
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, userId);

		Assert.Equal("NotSet", result.Value.Today.Members.Single().Status);
		Assert.False(result.Value.Today.Members.Single().IsVacation);

		var tomorrow = result.Value.Tomorrow.Members.Single();
		Assert.Equal(nameof(DayAvailabilityStatus.Off), tomorrow.Status);
		Assert.True(tomorrow.IsVacation);
		Assert.Null(tomorrow.From);
	}

	[Fact]
	public async Task Should_prefer_the_in_game_nickname_over_the_discord_display_name()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek", "sh4dro");
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, userId);

		Assert.Equal("sh4dro", result.Value.Today.Members.Single().InGameNickname);
	}

	[Fact]
	public async Task Should_attach_the_earliest_event_of_each_day_and_ignore_other_days()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");
		AddEvent(dbContext, "Trening wieczorny", Today, new TimeOnly(20, 0));
		AddEvent(dbContext, "Rozgrzewka", Today, new TimeOnly(9, 0));
		AddEvent(dbContext, "Mecz", Tomorrow, new TimeOnly(19, 0));
		AddEvent(dbContext, "Turniej", Tomorrow.AddDays(1), new TimeOnly(12, 0));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, userId);

		Assert.Equal("Rozgrzewka", result.Value.Today.Event?.Title);
		Assert.Equal("Mecz", result.Value.Tomorrow.Event?.Title);
	}

	[Fact]
	public async Task Should_exclude_guests_and_unassigned_non_coaches_but_keep_an_unassigned_coach()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var mainPlayerId = Guid.NewGuid();
		AddUser(dbContext, mainPlayerId, "Zenek");

		var guestId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = guestId,
			DiscordId = guestId.ToString(),
			DisplayName = "Nowy",
			AccessLevel = AccessLevel.Guest,
			CreatedAtUtc = DateTime.UtcNow
		});

		var unassignedPlayerId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = unassignedPlayerId,
			DiscordId = unassignedPlayerId.ToString(),
			DisplayName = "Pozostały",
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		});

		var unassignedCoachId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = unassignedCoachId,
			DiscordId = unassignedCoachId.ToString(),
			DisplayName = "Trener",
			AccessLevel = AccessLevel.Player,
			IsCoach = true,
			CreatedAtUtc = DateTime.UtcNow
		});

		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, mainPlayerId);

		var names = result.Value.Today.Members.Select(m => m.DisplayName).ToList();
		Assert.Contains("Zenek", names);
		Assert.Contains("Trener", names);
		Assert.DoesNotContain("Nowy", names);
		Assert.DoesNotContain("Pozostały", names);
	}

	[Fact]
	public async Task Should_average_the_users_5_most_recent_ratings_and_return_the_latest_match()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		AddUser(dbContext, userId, "Zenek");

		var (olderMatch, newerMatch) = (Guid.NewGuid(), Guid.NewGuid());
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = olderMatch, Opponent = "Team A", OurScore = 10, OpponentScore = 16, Category = MatchCategory.Scrimmage,
			PlayedAtUtc = DateTime.UtcNow.AddDays(-2), CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = newerMatch, Opponent = "Team B", OurScore = 16, OpponentScore = 7, MapName = "Mirage", Category = MatchCategory.Scrimmage,
			PlayedAtUtc = DateTime.UtcNow.AddDays(-1), CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(), MatchResultId = olderMatch, UserId = userId,
			Kills = 10, Deaths = 15, Assists = 2, Adr = 60, HeadshotPercentage = 30, Rating = 0.80,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(), MatchResultId = newerMatch, UserId = userId,
			Kills = 20, Deaths = 8, Assists = 4, Adr = 90, HeadshotPercentage = 40, Rating = 1.40,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await HandleAsync(dbContext, userId);

		Assert.False(result.IsError);
		Assert.NotNull(result.Value.MyRecentPerformance);
		Assert.Equal(1.10, result.Value.MyRecentPerformance!.AvgRating);
		Assert.Equal(2, result.Value.MyRecentPerformance.MatchesCounted);

		Assert.NotNull(result.Value.LastMatch);
		Assert.Equal("Team B", result.Value.LastMatch!.Opponent);
		Assert.True(result.Value.LastMatch.Won);
		Assert.Equal("Mirage", result.Value.LastMatch.MapName);
	}

	#endregion

	#region Private Methods

	private static Task<ErrorOr<DashboardSummaryDto>> HandleAsync(TestApplicationDbContext dbContext, Guid userId)
	{
		var handler = new GetDashboardSummaryHandler(dbContext, new TestCurrentUserService(userId));

		return handler.Handle(new GetDashboardSummaryQuery(), CancellationToken.None);
	}

	private static void AddUser(TestApplicationDbContext dbContext, Guid userId, string displayName, string? inGameNickname = null) =>
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString(),
			DisplayName = displayName,
			InGameNickname = inGameNickname,
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.Main,
			CreatedAtUtc = DateTime.UtcNow
		});

	private static void AddEvent(TestApplicationDbContext dbContext, string title, DateOnly date, TimeOnly time) =>
		dbContext.Events.Add(new Event
		{
			Id = Guid.NewGuid(),
			Title = title,
			Type = EventType.Training,
			StartsAtUtc = DateTime.SpecifyKind(date.ToDateTime(time), DateTimeKind.Utc),
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});

	#endregion
}
