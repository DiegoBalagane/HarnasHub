#region Usings

using FluentValidation.TestHelper;
using HarnasHub.Application.Features.Tactics.GetTacticEffectiveness;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetRoundReplay;
using HarnasHub.Tests.Application.Features.Tactics.GetMatchTacticMatches;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.GetTacticEffectiveness;

public class GetTacticEffectivenessHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_count_matched_rounds_and_wins_over_analysed_matches()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage);
		await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage);
		var tactic = GetMatchTacticMatchesHandlerTests.SeedTactic(dbContext, MapName.Mirage, MapSide.T, SiteA);
		var other = GetMatchTacticMatchesHandlerTests.SeedTactic(dbContext, MapName.Mirage, MapSide.T, SiteB);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, storage, new RoundSignatureCache()).Handle(new GetTacticEffectivenessQuery(MapName.Mirage), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.MapCalibrated);
		Assert.Equal(2, result.Value.MatchesAnalyzed);
		var played = result.Value.Tactics.Single(t => t.TacticId == tactic.Id);
		Assert.Equal((2, 2, 2), (played.RoundsPlayed, played.RoundsWon, played.Matches));
		Assert.Equal(0, result.Value.Tactics.Single(t => t.TacticId == other.Id).RoundsPlayed);
	}

	[Fact]
	public async Task Should_serve_cached_signatures_without_reading_storage_again()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage);
		GetMatchTacticMatchesHandlerTests.SeedTactic(dbContext, MapName.Mirage, MapSide.T, SiteA);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var cache = new RoundSignatureCache();
		await Handler(dbContext, storage, cache).Handle(new GetTacticEffectivenessQuery(MapName.Mirage), CancellationToken.None);

		var broken = new TestFileStorage(throwOnOpenRead: new IOException("gone"));
		var result = await Handler(dbContext, broken, cache).Handle(new GetTacticEffectivenessQuery(MapName.Mirage), CancellationToken.None);

		Assert.Equal(1, result.Value.MatchesAnalyzed);
		Assert.Equal(0, result.Value.MatchesSkipped);
		Assert.Equal(1, result.Value.RoundsMatched);
	}

	[Fact]
	public async Task Should_skip_matches_whose_timeline_cannot_be_read()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await GetRoundReplayHandlerTests.SeedAsync(dbContext, new TestFileStorage());
		var broken = new TestFileStorage(throwOnOpenRead: new IOException("gone"));

		var result = await Handler(dbContext, broken, new RoundSignatureCache()).Handle(new GetTacticEffectivenessQuery(MapName.Mirage), CancellationToken.None);

		Assert.Equal(0, result.Value.MatchesAnalyzed);
		Assert.Equal(1, result.Value.MatchesSkipped);
	}

	[Fact]
	public async Task Should_report_an_uncalibrated_map_with_zeroed_tactics()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		GetMatchTacticMatchesHandlerTests.SeedTactic(dbContext, MapName.Dust2, MapSide.T, SiteA);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, new TestFileStorage(), new RoundSignatureCache()).Handle(new GetTacticEffectivenessQuery(MapName.Dust2), CancellationToken.None);

		Assert.False(result.Value.MapCalibrated);
		Assert.Equal(0, Assert.Single(result.Value.Tactics).RoundsPlayed);
	}

	[Fact]
	public void Should_reject_an_unknown_map()
	{
		var validator = new GetTacticEffectivenessQueryValidator();

		validator.TestValidate(new GetTacticEffectivenessQuery((MapName)999)).ShouldHaveValidationErrorFor(x => x.Map);
		validator.TestValidate(new GetTacticEffectivenessQuery(MapName.Mirage)).ShouldNotHaveAnyValidationErrors();
	}

	#endregion

	#region Private Methods

	private static GetTacticEffectivenessHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage, RoundSignatureCache cache) =>
		new(dbContext, storage, cache, NullLogger<GetTacticEffectivenessHandler>.Instance);

	#endregion
}
