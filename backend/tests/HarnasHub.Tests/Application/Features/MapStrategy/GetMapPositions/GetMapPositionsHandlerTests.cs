using HarnasHub.Application.Features.MapStrategy.GetMapPositions;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.GetMapPositions;

public class GetMapPositionsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_only_the_requested_map_and_side_with_roster_details()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var awperId = await AddUserAsync(dbContext, "Zenon", "zen", TeamRole.AWPer);
		var iglId = await AddUserAsync(dbContext, "Adam", null, TeamRole.IGL);

		AddPosition(dbContext, MapName.Inferno, MapSide.CT, awperId, "Pit");
		AddPosition(dbContext, MapName.Inferno, MapSide.CT, iglId, "Arch");
		AddPosition(dbContext, MapName.Inferno, MapSide.T, awperId, "Banana");
		AddPosition(dbContext, MapName.Nuke, MapSide.CT, awperId, "Heaven");
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetMapPositionsHandler(dbContext);

		var result = await handler.Handle(new GetMapPositionsQuery(MapName.Inferno, MapSide.CT), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["Adam", "Zenon"], result.Value.Select(position => position.DisplayName));

		var awper = result.Value.Single(position => position.UserId == awperId.ToString());
		Assert.Equal("Pit", awper.Label);
		Assert.Equal("zen", awper.InGameNickname);
		Assert.Equal(nameof(TeamRole.AWPer), awper.TeamRole);
		Assert.Null(result.Value.Single(position => position.UserId == iglId.ToString()).InGameNickname);
	}

	[Fact]
	public async Task Should_return_an_empty_list_when_nothing_is_set_up()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetMapPositionsHandler(dbContext);

		var result = await handler.Handle(new GetMapPositionsQuery(MapName.Ancient, MapSide.T), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(result.Value);
	}

	#endregion

	#region Private Methods

	private static async Task<Guid> AddUserAsync(
		TestApplicationDbContext dbContext,
		string displayName,
		string? nickname,
		TeamRole teamRole)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = Guid.NewGuid().ToString(),
			DisplayName = displayName,
			Role = UserRole.Player,
			TeamRole = teamRole,
			InGameNickname = nickname,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		return user.Id;
	}

	private static void AddPosition(
		TestApplicationDbContext dbContext,
		MapName mapName,
		MapSide side,
		Guid userId,
		string label)
	{
		dbContext.MapPositionAssignments.Add(new MapPositionAssignment
		{
			Id = Guid.NewGuid(),
			MapName = mapName,
			Side = side,
			UserId = userId,
			Label = label,
			X = 0.5f,
			Y = 0.5f,
			UpdatedAtUtc = DateTime.UtcNow
		});
	}

	#endregion
}
