using HarnasHub.Application.Features.Roster.UpdateRosterSlot;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateRosterSlot;

public class UpdateRosterSlotHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_assign_the_roster_slot()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, "Zenek"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateRosterSlotHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateRosterSlotCommand(userId, RosterSlot.Main), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(RosterSlot.Main), result.Value.RosterSlot);
		Assert.Equal(RosterSlot.Main, (await dbContext.Users.SingleAsync()).RosterSlot);
	}

	[Fact]
	public async Task Should_reject_a_sixth_player_joining_main()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		for (var i = 0; i < 5; i++)
		{
			var existing = CreateUser(Guid.NewGuid(), $"Main{i}");
			existing.RosterSlot = RosterSlot.Main;
			dbContext.Users.Add(existing);
		}

		var candidateId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(candidateId, "Kandydat"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateRosterSlotHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateRosterSlotCommand(candidateId, RosterSlot.Main), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.MainRosterFull", result.FirstError.Code);
		Assert.Null((await dbContext.Users.SingleAsync(u => u.Id == candidateId)).RosterSlot);
	}

	[Fact]
	public async Task Should_allow_a_main_player_to_keep_their_own_spot()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		for (var i = 0; i < 4; i++)
		{
			var existing = CreateUser(Guid.NewGuid(), $"Main{i}");
			existing.RosterSlot = RosterSlot.Main;
			dbContext.Users.Add(existing);
		}

		var userId = Guid.NewGuid();
		var user = CreateUser(userId, "JużWMain");
		user.RosterSlot = RosterSlot.Main;
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateRosterSlotHandler(dbContext, new TestRealtimeNotifier());

		// Re-assigning Main to someone already in Main must not count against the cap (5 Main users total, this one included).
		var result = await handler.Handle(new UpdateRosterSlotCommand(userId, RosterSlot.Main), CancellationToken.None);

		Assert.False(result.IsError);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateRosterSlotHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateRosterSlotCommand(Guid.NewGuid(), RosterSlot.Bench), CancellationToken.None);

		Assert.True(result.IsError);
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
