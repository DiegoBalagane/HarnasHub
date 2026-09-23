using HarnasHub.Application.Features.Stats.GetPlayerLeaderboard;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Stats.GetPlayerLeaderboard;

public class GetPlayerLeaderboardHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_average_a_players_stats_across_their_matches()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString("N"),
			DisplayName = "Zenek",
			InGameNickname = "zen",
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		});
		var (matchA, matchB) = (Guid.NewGuid(), Guid.NewGuid());
		dbContext.MatchResults.Add(new MatchResult { Id = matchA, Opponent = "Foo", Category = MatchCategory.Scrimmage, PlayedAtUtc = DateTime.UtcNow, CreatedAtUtc = DateTime.UtcNow });
		dbContext.MatchResults.Add(new MatchResult { Id = matchB, Opponent = "Bar", Category = MatchCategory.Scrimmage, PlayedAtUtc = DateTime.UtcNow, CreatedAtUtc = DateTime.UtcNow });
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchA,
			UserId = userId,
			Kills = 20,
			Deaths = 10,
			Assists = 4,
			Adr = 80,
			HeadshotPercentage = 40,
			Rating = 1.20,
			KastPercentage = 70,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchB,
			UserId = userId,
			Kills = 10,
			Deaths = 20,
			Assists = 2,
			Adr = 60,
			HeadshotPercentage = 30,
			Rating = 0.80,
			KastPercentage = null,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetPlayerLeaderboardHandler(dbContext);

		var result = await handler.Handle(new GetPlayerLeaderboardQuery(null), CancellationToken.None);

		Assert.False(result.IsError);
		var entry = Assert.Single(result.Value);
		Assert.Equal("zen", entry.InGameNickname);
		Assert.Equal(2, entry.MatchesPlayed);
		Assert.Equal(15, entry.AvgKills);
		Assert.Equal(15, entry.AvgDeaths);
		Assert.Equal(1.00, entry.AvgRating);
		Assert.Equal(70, entry.AvgKastPercentage);
	}

	[Fact]
	public async Task Should_only_include_matches_of_the_requested_category()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString("N"),
			DisplayName = "Zenek",
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		});
		var (scrimMatch, leagueMatch) = (Guid.NewGuid(), Guid.NewGuid());
		dbContext.MatchResults.Add(new MatchResult { Id = scrimMatch, Opponent = "Foo", Category = MatchCategory.Scrimmage, PlayedAtUtc = DateTime.UtcNow, CreatedAtUtc = DateTime.UtcNow });
		dbContext.MatchResults.Add(new MatchResult { Id = leagueMatch, Opponent = "Bar", Category = MatchCategory.League, PlayedAtUtc = DateTime.UtcNow, CreatedAtUtc = DateTime.UtcNow });
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = scrimMatch,
			UserId = userId,
			Kills = 10,
			Deaths = 10,
			Assists = 0,
			Adr = 50,
			HeadshotPercentage = 20,
			Rating = 1.00,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = leagueMatch,
			UserId = userId,
			Kills = 30,
			Deaths = 5,
			Assists = 0,
			Adr = 120,
			HeadshotPercentage = 60,
			Rating = 1.80,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetPlayerLeaderboardHandler(dbContext);

		var result = await handler.Handle(new GetPlayerLeaderboardQuery(MatchCategory.League), CancellationToken.None);

		Assert.False(result.IsError);
		var entry = Assert.Single(result.Value);
		Assert.Equal(1, entry.MatchesPlayed);
		Assert.Equal(1.80, entry.AvgRating);
	}

	#endregion
}
