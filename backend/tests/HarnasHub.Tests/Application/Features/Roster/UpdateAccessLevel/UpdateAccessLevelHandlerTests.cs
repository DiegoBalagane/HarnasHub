using HarnasHub.Application.Features.Roster.UpdateAccessLevel;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateAccessLevel;

public class UpdateAccessLevelHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_access_level()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, "Zenek"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = CreateHandler(dbContext, callerId: Guid.NewGuid());

		var result = await handler.Handle(new UpdateAccessLevelCommand(userId, AccessLevel.Manager), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(AccessLevel.Manager), result.Value.Role);
		Assert.Equal(AccessLevel.Manager, (await dbContext.Users.SingleAsync()).AccessLevel);
	}

	[Fact]
	public async Task Should_leave_the_coach_tag_alone()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var user = CreateUser(userId, "TrenerZenek");
		user.IsCoach = true;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = CreateHandler(dbContext, callerId: Guid.NewGuid());

		var result = await handler.Handle(new UpdateAccessLevelCommand(userId, AccessLevel.Manager), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.IsCoach);
		Assert.True((await dbContext.Users.SingleAsync()).IsCoach);
	}

	[Fact]
	public async Task Should_reject_changing_your_own_access_level()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, "Zenek"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = CreateHandler(dbContext, callerId: userId);

		var result = await handler.Handle(new UpdateAccessLevelCommand(userId, AccessLevel.Guest), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.CannotChangeOwnRole", result.FirstError.Code);
		Assert.Equal(AccessLevel.Player, (await dbContext.Users.SingleAsync()).AccessLevel);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = CreateHandler(dbContext, callerId: Guid.NewGuid());

		var result = await handler.Handle(new UpdateAccessLevelCommand(Guid.NewGuid(), AccessLevel.Player), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static UpdateAccessLevelHandler CreateHandler(TestApplicationDbContext dbContext, Guid callerId) =>
		new(dbContext, new TestCurrentUserService(callerId, "Manager"), new TestRealtimeNotifier());

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
