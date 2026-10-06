#region Usings

using HarnasHub.Application.Features.MatchAnalysis.GetMapAnalytics;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchTimeline;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.GetMapAnalytics;

public class GetMapAnalyticsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_aggregate_only_matches_on_the_requested_map()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var onMirage = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, storage, DemoTimelineFactory.TeamA);
		var onDust = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, storage, DemoTimelineFactory.TeamA);
		onMirage.MapName = "Mirage";
		onDust.MapName = "Dust2";
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetMapAnalyticsHandler(dbContext, storage, NullLogger<GetMapAnalyticsHandler>.Instance);

		var result = await handler.Handle(new GetMapAnalyticsQuery(MapName.Mirage), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(1, result.Value.MatchesAnalyzed);
		Assert.Equal(1, result.Value.TSide.Total);
		Assert.Equal(1, result.Value.TSide.Won);
	}

	[Fact]
	public async Task Should_return_an_empty_aggregate_when_no_match_was_analysed()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetMapAnalyticsHandler(dbContext, new TestFileStorage(), NullLogger<GetMapAnalyticsHandler>.Instance);

		var result = await handler.Handle(new GetMapAnalyticsQuery(MapName.Mirage), CancellationToken.None);

		Assert.Equal(0, result.Value.MatchesAnalyzed);
	}

	[Fact]
	public async Task Should_count_unreadable_timelines_as_skipped()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, new TestFileStorage(), DemoTimelineFactory.TeamA);
		match.MapName = "Mirage";
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var broken = new TestFileStorage(throwOnOpenRead: new IOException("gone"));
		var handler = new GetMapAnalyticsHandler(dbContext, broken, NullLogger<GetMapAnalyticsHandler>.Instance);

		var result = await handler.Handle(new GetMapAnalyticsQuery(MapName.Mirage), CancellationToken.None);

		Assert.Equal(0, result.Value.MatchesAnalyzed);
		Assert.Equal(1, result.Value.MatchesSkipped);
	}

	#endregion
}
