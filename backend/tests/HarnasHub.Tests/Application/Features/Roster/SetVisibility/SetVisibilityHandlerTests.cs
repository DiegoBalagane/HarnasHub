using HarnasHub.Application.Features.Roster.SetVisibility;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetVisibility;

public class SetVisibilityHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public void Should_default_to_visible_in_both_places()
	{
		var user = CreateUser(_userId);

		Assert.True(user.ShowInStats);
		Assert.True(user.ShowInCalendar);
	}

	[Fact]
	public async Task Should_store_both_flags_and_notify_every_affected_topic()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var result = await new SetVisibilityHandler(dbContext, notifier)
			.Handle(new SetVisibilityCommand(_userId, false, true), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.False(result.Value.ShowInStats);
		Assert.True(result.Value.ShowInCalendar);
		var stored = await dbContext.Users.SingleAsync();
		Assert.False(stored.ShowInStats);
		Assert.True(stored.ShowInCalendar);
		Assert.Equal(["roster", "availability-week", "dashboard", "stats"], notifier.Topics);
	}

	[Fact]
	public async Task Should_allow_switching_visibility_back_on()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId);
		user.ShowInStats = false;
		user.ShowInCalendar = false;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new SetVisibilityHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new SetVisibilityCommand(_userId, true, true), CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.Users.SingleAsync();
		Assert.True(stored.ShowInStats);
		Assert.True(stored.ShowInCalendar);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_user_does_not_exist()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new SetVisibilityHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new SetVisibilityCommand(Guid.NewGuid(), false, false), CancellationToken.None);

		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Zawodnik",
		AccessLevel = AccessLevel.Player,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
