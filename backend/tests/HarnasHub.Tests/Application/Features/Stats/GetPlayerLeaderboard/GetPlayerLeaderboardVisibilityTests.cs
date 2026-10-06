using HarnasHub.Application.Features.Stats.GetPlayerLeaderboard;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Stats.GetPlayerLeaderboard;

public class GetPlayerLeaderboardVisibilityTests
{
	#region Public Methods

	[Fact]
	public async Task Should_leave_out_players_hidden_from_stats_but_keep_their_data()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var shown = AddUser(dbContext, "Widoczny", showInStats: true);
		var hidden = AddUser(dbContext, "Ukryty", showInStats: false);
		var matchId = Guid.NewGuid();
		dbContext.MatchResults.Add(new MatchResult { Id = matchId, Opponent = "Foo", Category = MatchCategory.Scrimmage, PlayedAtUtc = DateTime.UtcNow, CreatedAtUtc = DateTime.UtcNow });
		dbContext.PlayerMatchStats.AddRange(Stat(matchId, shown), Stat(matchId, hidden));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetPlayerLeaderboardHandler(dbContext).Handle(new GetPlayerLeaderboardQuery(null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal([shown], result.Value.Select(entry => entry.UserId));
		Assert.Equal(2, dbContext.PlayerMatchStats.Count());
	}

	#endregion

	#region Private Methods

	private static Guid AddUser(TestApplicationDbContext dbContext, string name, bool showInStats)
	{
		var id = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = id,
			DiscordId = id.ToString("N"),
			DisplayName = name,
			AccessLevel = AccessLevel.Player,
			ShowInStats = showInStats,
			CreatedAtUtc = DateTime.UtcNow
		});
		return id;
	}

	private static PlayerMatchStat Stat(Guid matchId, Guid userId) => new()
	{
		Id = Guid.NewGuid(),
		MatchResultId = matchId,
		UserId = userId,
		Kills = 20,
		Deaths = 10,
		Assists = 4,
		Adr = 80,
		HeadshotPercentage = 40,
		Rating = 1.2,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
