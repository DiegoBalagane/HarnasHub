using HarnasHub.Application.Features.Results.DeleteResult;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.DeleteResult;

public class DeleteResultHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_result_and_its_stat_lines()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matchId = Guid.NewGuid();
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = matchId,
			Opponent = "Team X",
			OurScore = 16,
			OpponentScore = 10,
			Category = MatchCategory.Scrimmage,
			PlayedAtUtc = DateTime.UtcNow,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchId,
			UserId = Guid.NewGuid(),
			Kills = 20,
			Deaths = 10,
			Assists = 5,
			Adr = 85.5,
			HeadshotPercentage = 40,
			Rating = 1.2,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new DeleteResultHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteResultCommand(matchId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.MatchResults);
		Assert.Empty(dbContext.PlayerMatchStats);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteResultHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteResultCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
	}

	#endregion
}
