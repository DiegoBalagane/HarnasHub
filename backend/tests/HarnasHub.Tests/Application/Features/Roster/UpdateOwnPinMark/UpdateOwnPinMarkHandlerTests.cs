using HarnasHub.Application.Features.Roster.UpdateOwnPinMark;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnPinMark;

public class UpdateOwnPinMarkHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_mark_for_any_roster_member()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, RosterSlot.Bench));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnPinMarkHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnPinMarkCommand("9"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("9", result.Value.PinMark);
		Assert.Equal("9", (await dbContext.Users.SingleAsync()).PinMark);
	}

	[Fact]
	public async Task Should_clear_the_mark_when_given_an_empty_value()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId, RosterSlot.Main);
		user.PinMark = "K";
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnPinMarkHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnPinMarkCommand(null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null((await dbContext.Users.SingleAsync()).PinMark);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id, RosterSlot? rosterSlot) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Zawodnik",
		Role = UserRole.Player,
		RosterSlot = rosterSlot,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
