using HarnasHub.Application.Features.Roster.SetSteamId64;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetSteamId64;

public class SetSteamId64HandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_steam_id_for_a_team_member()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new SetSteamId64Handler(dbContext, notifier);

		var result = await handler.Handle(new SetSteamId64Command(_userId, "76561198012345678"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("76561198012345678", (await dbContext.Users.SingleAsync()).SteamId64);
		Assert.Equal(["roster"], notifier.Topics);
	}

	[Fact]
	public async Task Should_clear_the_steam_id_with_a_null_value()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId);
		user.SteamId64 = "76561198012345678";
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetSteamId64Handler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetSteamId64Command(_userId, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null((await dbContext.Users.SingleAsync()).SteamId64);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_user_does_not_exist()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetSteamId64Handler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetSteamId64Command(Guid.NewGuid(), "76561198012345678"), CancellationToken.None);

		Assert.True(result.IsError);
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
