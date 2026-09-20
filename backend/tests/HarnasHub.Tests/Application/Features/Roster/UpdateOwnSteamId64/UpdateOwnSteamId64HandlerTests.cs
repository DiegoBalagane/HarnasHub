using HarnasHub.Application.Features.Roster.UpdateOwnSteamId64;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnSteamId64;

public class UpdateOwnSteamId64HandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_callers_own_steam_id_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateOwnSteamId64Handler(dbContext, new TestCurrentUserService(_userId), notifier);

		var result = await handler.Handle(new UpdateOwnSteamId64Command("76561198012345678"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("76561198012345678", result.Value.SteamId64);
		Assert.Equal("76561198012345678", (await dbContext.Users.SingleAsync()).SteamId64);
		Assert.Equal(["roster"], notifier.Topics);
	}

	[Fact]
	public async Task Should_clear_the_steam_id_when_given_null()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId);
		user.SteamId64 = "76561198012345678";
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnSteamId64Handler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnSteamId64Command(null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.SteamId64);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_caller_has_no_account()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateOwnSteamId64Handler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnSteamId64Command("76561198012345678"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Harnas",
		AccessLevel = AccessLevel.Player,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
