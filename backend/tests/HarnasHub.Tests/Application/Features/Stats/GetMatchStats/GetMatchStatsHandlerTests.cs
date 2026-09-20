using HarnasHub.Application.Features.Stats.GetMatchStats;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Stats.GetMatchStats;

public class GetMatchStatsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_the_display_name_for_an_existing_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matchId = Guid.NewGuid();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString("N"),
			DisplayName = "Zenek",
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchId,
			UserId = userId,
			Kills = 20,
			Deaths = 10,
			Assists = 5,
			Adr = 85.5,
			HeadshotPercentage = 40,
			Rating = 1.2,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetMatchStatsHandler(dbContext);

		var result = await handler.Handle(new GetMatchStatsQuery(matchId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Zenek", result.Value.Single().DisplayName);
	}

	[Fact]
	public async Task Should_fall_back_to_a_placeholder_name_when_the_player_was_deleted()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matchId = Guid.NewGuid();
		var deletedUserId = Guid.NewGuid();
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchId,
			UserId = deletedUserId,
			Kills = 20,
			Deaths = 10,
			Assists = 5,
			Adr = 85.5,
			HeadshotPercentage = 40,
			Rating = 1.2,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetMatchStatsHandler(dbContext);

		var result = await handler.Handle(new GetMatchStatsQuery(matchId), CancellationToken.None);

		Assert.False(result.IsError);
		var stat = result.Value.Single();
		Assert.Equal(deletedUserId, stat.UserId);
		Assert.Equal("Usunięty zawodnik", stat.DisplayName);
	}

	[Fact]
	public async Task Should_show_the_demo_name_for_a_player_never_connected_to_a_roster_account()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matchId = Guid.NewGuid();
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchId,
			UserId = null,
			DemoPlayerName = "shadow",
			Kills = 20,
			Deaths = 10,
			Assists = 5,
			Adr = 85.5,
			HeadshotPercentage = 40,
			Rating = 1.2,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetMatchStatsHandler(dbContext);

		var result = await handler.Handle(new GetMatchStatsQuery(matchId), CancellationToken.None);

		Assert.False(result.IsError);
		var stat = result.Value.Single();
		Assert.Null(stat.UserId);
		Assert.Equal("shadow", stat.DisplayName);
	}

	#endregion
}
