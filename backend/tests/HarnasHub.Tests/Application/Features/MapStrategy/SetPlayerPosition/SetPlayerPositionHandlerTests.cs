using HarnasHub.Application.Features.MapStrategy.SetPlayerPosition;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.SetPlayerPosition;

public class SetPlayerPositionHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_insert_a_position_and_notify_when_the_player_has_none_on_that_map_side()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await SeedUserAsync(dbContext);
		var notifier = new TestRealtimeNotifier();
		var handler = new SetPlayerPositionHandler(dbContext, notifier);

		var command = new SetPlayerPositionCommand(MapName.Mirage, MapSide.CT, _userId, "Window", 0.25f, 0.4f, "Trzyma mida");

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Window", result.Value.Label);
		Assert.Equal("s1mple", result.Value.InGameNickname);
		Assert.Equal(nameof(TeamRole.AWPer), result.Value.TeamRole);

		var stored = await dbContext.MapPositionAssignments.SingleAsync();
		Assert.Equal(MapName.Mirage, stored.MapName);
		Assert.Equal(MapSide.CT, stored.Side);
		Assert.Equal(_userId, stored.UserId);
		Assert.Equal(0.25f, stored.X);
		Assert.Equal(0.4f, stored.Y);
		Assert.Equal("Trzyma mida", stored.Note);
		Assert.Equal(["map-strategy"], notifier.Topics);
	}

	[Fact]
	public async Task Should_overwrite_the_existing_position_for_the_same_map_side_and_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await SeedUserAsync(dbContext);
		var existingId = Guid.NewGuid();
		dbContext.MapPositionAssignments.Add(new MapPositionAssignment
		{
			Id = existingId,
			MapName = MapName.Mirage,
			Side = MapSide.CT,
			UserId = _userId,
			Label = "Window",
			X = 0.25f,
			Y = 0.4f,
			Note = "Trzyma mida",
			UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetPlayerPositionHandler(dbContext, new TestRealtimeNotifier());

		var command = new SetPlayerPositionCommand(MapName.Mirage, MapSide.CT, _userId, "Connector", 0.6f, 0.7f, null);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(existingId, result.Value.Id);

		var stored = await dbContext.MapPositionAssignments.SingleAsync();
		Assert.Equal("Connector", stored.Label);
		Assert.Equal(0.6f, stored.X);
		Assert.Equal(0.7f, stored.Y);
		Assert.Null(stored.Note);
	}

	[Fact]
	public async Task Should_keep_the_same_player_position_on_the_other_side_untouched()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await SeedUserAsync(dbContext);
		dbContext.MapPositionAssignments.Add(new MapPositionAssignment
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Side = MapSide.T,
			UserId = _userId,
			Label = "Ramp",
			X = 0.1f,
			Y = 0.1f,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetPlayerPositionHandler(dbContext, new TestRealtimeNotifier());

		await handler.Handle(
			new SetPlayerPositionCommand(MapName.Mirage, MapSide.CT, _userId, "Window", 0.25f, 0.4f, null),
			CancellationToken.None);

		var rows = await dbContext.MapPositionAssignments.ToListAsync();
		Assert.Equal(2, rows.Count);
		Assert.Equal("Ramp", rows.Single(row => row.Side == MapSide.T).Label);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_player_does_not_exist()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new SetPlayerPositionHandler(dbContext, notifier);

		var command = new SetPlayerPositionCommand(MapName.Nuke, MapSide.T, Guid.NewGuid(), null, 0.5f, 0.5f, null);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("MapStrategy.UserNotFound", result.FirstError.Code);
		Assert.Empty(await dbContext.MapPositionAssignments.ToListAsync());
		Assert.Empty(notifier.Topics);
	}

	#endregion

	#region Private Methods

	private async Task SeedUserAsync(TestApplicationDbContext dbContext)
	{
		dbContext.Users.Add(new User
		{
			Id = _userId,
			DiscordId = "1",
			DisplayName = "Kacper",
			AccessLevel = AccessLevel.Player,
			TeamRole = Core.Enums.TeamRole.AWPer,
			InGameNickname = "s1mple",
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);
	}

	#endregion
}
