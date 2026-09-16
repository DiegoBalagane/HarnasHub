using HarnasHub.Application.Features.Roster.UpdateOwnNickname;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnNickname;

public class UpdateOwnNicknameHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_trimmed_nickname_of_the_caller_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, "Harnas"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateOwnNicknameHandler(dbContext, new TestCurrentUserService(_userId), notifier);

		var result = await handler.Handle(new UpdateOwnNicknameCommand("  s1mple  "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("s1mple", result.Value.InGameNickname);
		Assert.Equal("s1mple", (await dbContext.Users.SingleAsync()).InGameNickname);
		Assert.Equal(["roster"], notifier.Topics);
	}

	[Fact]
	public async Task Should_never_touch_another_member()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var otherUserId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(_userId, "Harnas"));
		dbContext.Users.Add(CreateUser(otherUserId, "Inny"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnNicknameHandler(
			dbContext,
			new TestCurrentUserService(_userId),
			new TestRealtimeNotifier());

		await handler.Handle(new UpdateOwnNicknameCommand("s1mple"), CancellationToken.None);

		var other = await dbContext.Users.SingleAsync(user => user.Id == otherUserId);
		Assert.Null(other.InGameNickname);
	}

	[Fact]
	public async Task Should_clear_the_nickname_back_to_the_discord_name_when_given_a_blank_value()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId, "Harnas");
		user.InGameNickname = "StaryNick";
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnNicknameHandler(
			dbContext,
			new TestCurrentUserService(_userId),
			new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnNicknameCommand("   "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.InGameNickname);
		Assert.Null((await dbContext.Users.SingleAsync()).InGameNickname);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_caller_has_no_account()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateOwnNicknameHandler(
			dbContext,
			new TestCurrentUserService(_userId),
			new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnNicknameCommand("s1mple"), CancellationToken.None);

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
		Role = UserRole.Player,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
