#region Usings

using HarnasHub.Application.Features.MatchAnalysis.GetMatchAnalysis;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchTimeline;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchAnalysis;

public class GetMatchAnalysisHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_analyse_the_stored_timeline_without_a_library()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, storage, DemoTimelineFactory.TeamA);
		var handler = new GetMatchAnalysisHandler(dbContext, storage, NullLogger<GetMatchAnalysisHandler>.Instance);

		var result = await handler.Handle(new GetMatchAnalysisQuery(match.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Mirage", result.Value.MapName);
		Assert.True(result.Value.OurTeamResolved);
		Assert.Null(result.Value.Grenades);
	}

	[Fact]
	public async Task Should_compare_with_pinned_library_nades_of_the_map()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await GetMatchTimelineHandlerTests.SeedAsync(dbContext, storage, DemoTimelineFactory.TeamA);
		dbContext.NadeEntries.Add(new NadeEntry { Id = Guid.NewGuid(), MapName = MapName.Mirage, Type = GrenadeType.Smoke, Title = "Window", LandingX = 0.4f, LandingY = 0.4f });
		dbContext.NadeEntries.Add(new NadeEntry { Id = Guid.NewGuid(), MapName = MapName.Mirage, Type = GrenadeType.Smoke, Title = "No pin" });
		dbContext.NadeEntries.Add(new NadeEntry { Id = Guid.NewGuid(), MapName = MapName.Dust2, Type = GrenadeType.Smoke, Title = "Other map", LandingX = 0.1f, LandingY = 0.1f });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetMatchAnalysisHandler(dbContext, storage, NullLogger<GetMatchAnalysisHandler>.Instance);

		var result = await handler.Handle(new GetMatchAnalysisQuery(match.Id), CancellationToken.None);

		Assert.Equal(1, result.Value.Grenades!.TrainedTotal);
		Assert.Equal(0, result.Value.Grenades.TrainedThrown);
	}

	[Fact]
	public async Task Should_propagate_a_missing_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result();
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetMatchAnalysisHandler(dbContext, new TestFileStorage(), NullLogger<GetMatchAnalysisHandler>.Instance);

		var result = await handler.Handle(new GetMatchAnalysisQuery(match.Id), CancellationToken.None);

		Assert.Equal("MatchAnalysis.TimelineNotFound", result.FirstError.Code);
	}

	#endregion
}
