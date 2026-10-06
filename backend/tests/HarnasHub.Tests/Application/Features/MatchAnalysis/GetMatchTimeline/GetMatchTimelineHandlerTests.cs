#region Usings

using HarnasHub.Application.Features.MatchAnalysis.GetMatchTimeline;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.GetMatchTimeline;

public class GetMatchTimelineHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_match()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetMatchTimelineHandler(dbContext, new TestFileStorage(), NullLogger<GetMatchTimelineHandler>.Instance);

		var result = await handler.Handle(new GetMatchTimelineQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_match_has_no_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result();
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetMatchTimelineHandler(dbContext, new TestFileStorage(), NullLogger<GetMatchTimelineHandler>.Instance);

		var result = await handler.Handle(new GetMatchTimelineQuery(match.Id), CancellationToken.None);

		Assert.Equal("MatchAnalysis.TimelineNotFound", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_load_the_stored_timeline_with_the_stored_team_as_us()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await SeedAsync(dbContext, storage, DemoTimelineFactory.TeamB);
		var handler = new GetMatchTimelineHandler(dbContext, storage, NullLogger<GetMatchTimelineHandler>.Instance);

		var result = await handler.Handle(new GetMatchTimelineQuery(match.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.OurTeamResolved);
		Assert.Equal("Mirage", result.Value.MapName);
		Assert.Equal(MapSide.CT, result.Value.Rounds[0].OurSide);
		Assert.False(result.Value.Rounds[0].WeWon);
	}

	[Fact]
	public async Task Should_resolve_us_from_the_recorded_score_when_no_team_was_stored()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await SeedAsync(dbContext, storage, [], ourScore: 0, opponentScore: 1);
		var handler = new GetMatchTimelineHandler(dbContext, storage, NullLogger<GetMatchTimelineHandler>.Instance);

		var result = await handler.Handle(new GetMatchTimelineQuery(match.Id), CancellationToken.None);

		Assert.True(result.Value.OurTeamResolved);
		Assert.Equal(MapSide.CT, result.Value.Rounds[0].OurSide);
	}

	[Fact]
	public async Task Should_fail_gracefully_when_the_stored_file_cannot_be_read()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = await SeedAsync(dbContext, new TestFileStorage(), DemoTimelineFactory.TeamA);
		var handler = new GetMatchTimelineHandler(dbContext, new TestFileStorage(throwOnOpenRead: new IOException("gone")), NullLogger<GetMatchTimelineHandler>.Instance);

		var result = await handler.Handle(new GetMatchTimelineQuery(match.Id), CancellationToken.None);

		Assert.Equal("MatchAnalysis.TimelineUnavailable", result.FirstError.Code);
	}

	#endregion

	#region Internal Methods

	/// <summary>Seeds a match won 1:0 by the T side in its only round, with its timeline stored in <paramref name="storage"/>.</summary>
	internal static async Task<MatchResult> SeedAsync(
		TestApplicationDbContext dbContext, TestFileStorage storage, long[] ourTeam, int ourScore = 1, int opponentScore = 0)
	{
		var match = MatchTimelineFactory.Result(ourScore, opponentScore);
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var timeline = MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]);
		await MatchTimelineAttacher.AttachAsync(dbContext, storage, match.Id, timeline, DemoTimelineSerializer.CurrentParserVersion, ourTeam, CancellationToken.None);
		return match;
	}

	#endregion
}
