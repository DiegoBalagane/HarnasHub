using HarnasHub.Application.Features.Roster.SetPinColor;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetPinColor;

public class SetPinColorHandlerTests
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

		var notifier = new TestRealtimeNotifier();
		var handler = new SetPinColorHandler(dbContext, notifier);

		var result = await handler.Handle(new SetPinColorCommand(_userId, PinColor.Blue), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(PinColor.Blue), result.Value.PinColor);
		Assert.Equal(PinColor.Blue, (await dbContext.Users.SingleAsync()).PinColor);
		Assert.Equal(["roster", "map-strategy"], notifier.Topics);
	}

	[Fact]
	public async Task Should_reject_a_color_for_a_bench_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, RosterSlot.Bench));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetPinColorHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetPinColorCommand(_userId, PinColor.Blue), CancellationToken.None);

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

		var handler = new SetPinColorHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetPinColorCommand(_userId, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null((await dbContext.Users.SingleAsync()).PinColor);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_user_does_not_exist()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetPinColorHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new SetPinColorCommand(Guid.NewGuid(), PinColor.Green), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id, RosterSlot? rosterSlot) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Zawodnik",
		AccessLevel = AccessLevel.Player,
		RosterSlot = rosterSlot,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
