#region Usings

using HarnasHub.Application.Features.Tactics.GetMatchTacticMatches;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetRoundReplay;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.GetMatchTacticMatches;

public class GetMatchTacticMatchesHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_match()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage()).Handle(new GetMatchTacticMatchesQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_match_our_round_to_the_tactic_whose_points_we_stood_on()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage);
		var tactic = SeedTactic(dbContext, MapName.Mirage, MapSide.T, SiteA);
		SeedTactic(dbContext, MapName.Mirage, MapSide.CT, SiteA);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, storage).Handle(new GetMatchTacticMatchesQuery(match.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.MapCalibrated);
		Assert.True(result.Value.HasPositions);
		var round = Assert.Single(result.Value.Rounds);
		Assert.Equal(tactic.Id, round.TacticId);
		Assert.Equal(MapSide.T, round.Side);
		Assert.Equal(100, round.ScorePercent);
	}

	[Fact]
	public async Task Should_report_an_uncalibrated_map_without_matching()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var timeline = MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]) with { MapName = MapName.Inferno };
		var match = await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage, timeline);
		SeedTactic(dbContext, MapName.Inferno, MapSide.T, SiteA);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, storage).Handle(new GetMatchTacticMatchesQuery(match.Id), CancellationToken.None);

		Assert.False(result.Value.MapCalibrated);
		Assert.Empty(result.Value.Rounds);
	}

	#endregion

	#region Internal Methods

	/// <summary>Adds a tactic with a single position point at <paramref name="point"/> (not saved yet).</summary>
	internal static Tactic SeedTactic(TestApplicationDbContext dbContext, MapName map, MapSide side, (float X, float Y) point)
	{
		var tactic = new Tactic
		{
			Id = Guid.NewGuid(),
			MapName = map,
			Side = side,
			Name = $"{side} tactic",
			CreatedAtUtc = DateTime.UtcNow,
			Points = [new TacticPoint { Id = Guid.NewGuid(), Order = 1, X = point.X, Y = point.Y }]
		};
		dbContext.Tactics.Add(tactic);
		return tactic;
	}

	#endregion

	#region Private Methods

	private static GetMatchTacticMatchesHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage) =>
		new(dbContext, storage, NullLogger<GetMatchTacticMatchesHandler>.Instance);

	#endregion
}
