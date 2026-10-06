using HarnasHub.Application.Features.Availability.GetWeekAvailability;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.GetWeekAvailability;

public class GetWeekAvailabilityVisibilityTests
{
	#region Private Fields

	private static readonly DateOnly WeekStart = new(2026, 9, 14);

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_leave_out_players_hidden_from_the_calendar_for_other_viewers()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddUser(dbContext, "Widoczny", showInCalendar: true);
		AddUser(dbContext, "Ukryty", showInCalendar: false);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()));
		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		Assert.Equal(["Widoczny"], result.Value.Members.Select(member => member.DisplayName));
		Assert.All(result.Value.Members, member => Assert.False(member.HiddenFromCalendar));
	}

	[Fact]
	public async Task Should_still_return_a_hidden_players_own_row_flagged_as_hidden()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var hiddenId = AddUser(dbContext, "Ukryty", showInCalendar: false);
		AddUser(dbContext, "Widoczny", showInCalendar: true);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetWeekAvailabilityHandler(dbContext, new TestCurrentUserService(hiddenId));
		var result = await handler.Handle(new GetWeekAvailabilityQuery(WeekStart), CancellationToken.None);

		var own = Assert.Single(result.Value.Members, member => member.UserId == hiddenId);
		Assert.True(own.HiddenFromCalendar);
		Assert.Equal(2, result.Value.Members.Count);
	}

	#endregion

	#region Private Methods

	private static Guid AddUser(TestApplicationDbContext dbContext, string name, bool showInCalendar)
	{
		var id = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = id,
			DiscordId = id.ToString("N"),
			DisplayName = name,
			AccessLevel = AccessLevel.Player,
			RosterSlot = RosterSlot.Main,
			ShowInCalendar = showInCalendar,
			CreatedAtUtc = DateTime.UtcNow
		});
		return id;
	}

	#endregion
}
