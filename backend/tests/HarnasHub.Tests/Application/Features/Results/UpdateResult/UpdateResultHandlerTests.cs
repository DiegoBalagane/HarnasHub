using HarnasHub.Application.Features.Results.UpdateResult;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.UpdateResult;

public class UpdateResultHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_results_metadata()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matchId = Guid.NewGuid();
		var playedAt = new DateTime(2026, 8, 19, 0, 0, 0, DateTimeKind.Utc);
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = matchId,
			Opponent = "Team X",
			OurScore = 16,
			OpponentScore = 10,
			MapName = "Mirage",
			Category = MatchCategory.Scrimmage,
			PlayedAtUtc = playedAt,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateResultHandler(dbContext, notifier);
		var newPlayedAt = new DateTime(2026, 9, 23, 0, 0, 0, DateTimeKind.Utc);

		var command = new UpdateResultCommand(
			matchId, "Team Y", 13, 7, "Ancient", null, "Poprawiono", newPlayedAt, MatchCategory.Scrimmage, null, null);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Team Y", result.Value.Opponent);
		Assert.Equal(13, result.Value.OurScore);
		Assert.Equal(7, result.Value.OpponentScore);
		Assert.Equal("Ancient", result.Value.MapName);
		Assert.Equal(newPlayedAt, result.Value.PlayedAtUtc);
		Assert.Equal(["results", "dashboard"], notifier.Topics);

		var stored = dbContext.MatchResults.Single();
		Assert.Equal("Team Y", stored.Opponent);
		Assert.Equal("Ancient", stored.MapName);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateResultHandler(dbContext, new TestRealtimeNotifier());

		var command = new UpdateResultCommand(
			Guid.NewGuid(), "Team Y", 13, 7, null, null, null, DateTime.UtcNow, MatchCategory.Scrimmage, null, null);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
	}

	#endregion
}
