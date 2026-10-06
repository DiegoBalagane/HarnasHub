#region Usings

using HarnasHub.Application.Features.MatchAnalysis.GetMatchInsights;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchTimeline;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchInsights;

public class GetMatchInsightsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_build_insights_from_the_stored_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, storage, DemoTimelineFactory.TeamA);
		var handler = new GetMatchInsightsHandler(dbContext, storage, NullLogger<GetMatchInsightsHandler>.Instance);

		var result = await handler.Handle(new GetMatchInsightsQuery(match.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Contains(result.Value, i => i.Code == "pistols");
	}

	[Fact]
	public async Task Should_propagate_a_missing_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result();
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetMatchInsightsHandler(dbContext, new TestFileStorage(), NullLogger<GetMatchInsightsHandler>.Instance);

		var result = await handler.Handle(new GetMatchInsightsQuery(match.Id), CancellationToken.None);

		Assert.Equal("MatchAnalysis.TimelineNotFound", result.FirstError.Code);
	}

	#endregion
}
