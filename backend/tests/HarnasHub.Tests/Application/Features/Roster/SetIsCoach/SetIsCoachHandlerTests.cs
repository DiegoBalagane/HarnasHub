using HarnasHub.Application.Features.Roster.SetIsCoach;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetIsCoach;

public class SetIsCoachHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_tag_the_member_as_coach()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, "Zenek"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetIsCoachHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetIsCoachCommand(userId, true), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.IsCoach);
		Assert.True((await dbContext.Users.SingleAsync()).IsCoach);
	}

	[Fact]
	public async Task Should_clear_the_coach_tag()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var user = CreateUser(userId, "Zenek");
		user.IsCoach = true;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetIsCoachHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetIsCoachCommand(userId, false), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.False(result.Value.IsCoach);
		Assert.False((await dbContext.Users.SingleAsync()).IsCoach);
	}

	[Fact]
	public async Task Should_keep_the_access_level_untouched()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var user = CreateUser(userId, "Menedżer");
		user.AccessLevel = AccessLevel.Manager;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetIsCoachHandler(dbContext, new TestRealtimeNotifier());

		// A Manager must be able to be the coach as well — the two concepts are independent.
		var result = await handler.Handle(new SetIsCoachCommand(userId, true), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(AccessLevel.Manager), result.Value.Role);
		Assert.True(result.Value.IsCoach);
		Assert.Equal(AccessLevel.Manager, (await dbContext.Users.SingleAsync()).AccessLevel);
	}

	[Fact]
	public async Task Should_reject_tagging_a_guest_as_coach()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var user = CreateUser(userId, "Nowy");
		user.AccessLevel = AccessLevel.Guest;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetIsCoachHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetIsCoachCommand(userId, true), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.GuestCannotBeCoach", result.FirstError.Code);
		Assert.False((await dbContext.Users.SingleAsync()).IsCoach);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetIsCoachHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetIsCoachCommand(Guid.NewGuid(), true), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id, string displayName) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = displayName,
		AccessLevel = AccessLevel.Player,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
