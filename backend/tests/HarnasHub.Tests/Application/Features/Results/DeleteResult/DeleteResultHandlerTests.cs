using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Results.DeleteResult;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchTimeline;
using HarnasHub.Tests.Application.Features.Tactics;
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

		var handler = new DeleteResultHandler(dbContext, new TestRealtimeNotifier(), new TestFileStorage());

		var result = await handler.Handle(new DeleteResultCommand(matchId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.MatchResults);
		Assert.Empty(dbContext.PlayerMatchStats);
	}

	[Fact]
	public async Task Should_delete_the_demo_timeline_row_and_its_stored_file()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, storage, DemoTimelineFactory.TeamA);
		var handler = new DeleteResultHandler(dbContext, new TestRealtimeNotifier(), storage);

		var result = await handler.Handle(new DeleteResultCommand(match.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.MatchDemoAnalyses);
		Assert.Contains(MatchTimelineStorage.MatchKey(match.Id), storage.DeletedKeys);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteResultHandler(dbContext, new TestRealtimeNotifier(), new TestFileStorage());

		var result = await handler.Handle(new DeleteResultCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
	}

	#endregion
}
