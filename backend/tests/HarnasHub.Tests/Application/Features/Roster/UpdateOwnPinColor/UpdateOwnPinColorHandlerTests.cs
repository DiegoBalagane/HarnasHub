using HarnasHub.Application.Features.Roster.UpdateOwnPinColor;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateOwnPinColor;

public class UpdateOwnPinColorHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_pin_color_for_a_main_roster_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, RosterSlot.Main));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnPinColorHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnPinColorCommand(PinColor.Blue), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(PinColor.Blue), result.Value.PinColor);
		Assert.Equal(PinColor.Blue, (await dbContext.Users.SingleAsync()).PinColor);
	}

	[Fact]
	public async Task Should_reject_a_color_for_a_bench_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, RosterSlot.Bench));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnPinColorHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnPinColorCommand(PinColor.Blue), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.PinColorRequiresMainRoster", result.FirstError.Code);
		Assert.Null((await dbContext.Users.SingleAsync()).PinColor);
	}

	[Fact]
	public async Task Should_allow_clearing_the_color_even_off_main_roster()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = CreateUser(_userId, RosterSlot.Bench);
		user.PinColor = PinColor.Orange;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateOwnPinColorHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateOwnPinColorCommand(null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null((await dbContext.Users.SingleAsync()).PinColor);
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
