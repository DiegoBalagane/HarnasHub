#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.GetRoundReplay;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.OpponentReport;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.GetRoundReplay;

public class GetRoundReplayHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_match()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage()).Handle(new GetRoundReplayQuery(Guid.NewGuid(), 1), CancellationToken.None);

		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_round_the_match_did_not_have()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await SeedAsync(dbContext, storage);

		var result = await Handler(dbContext, storage).Handle(new GetRoundReplayQuery(match.Id, 7), CancellationToken.None);

		Assert.Equal(MatchAnalysisErrors.RoundNotFound.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_replay_the_round_from_our_perspective()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var match = await SeedAsync(dbContext, storage);

		var result = await Handler(dbContext, storage).Handle(new GetRoundReplayQuery(match.Id, 1), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Team X", result.Value.OpponentName);
		Assert.Equal(MapSide.T, result.Value.OurSide);
		Assert.Equal(ReplayPositionsStatus.Available, result.Value.PositionsStatus);
		var player = result.Value.Players.Single(p => p.SteamId64 == "1");
		Assert.Equal(ReplayTeam.Ours, player.Team);
		Assert.Equal(ReplayTeam.Opponent, result.Value.Players.Single(p => p.SteamId64 == "3").Team);
		Assert.Contains(result.Value.Frames[3].Players, p => p.Player == player.Index);
	}

	#endregion

	#region Internal Methods

	/// <summary>Seeds a Mirage match against "Team X" whose single round has team A (1, 2 — us) on T with player 1 sampled.</summary>
	internal static async Task<MatchResult> SeedAsync(TestApplicationDbContext dbContext, TestFileStorage storage, DemoTimeline? timeline = null)
	{
		var match = MatchTimelineFactory.Result();
		match.MapName = "Mirage";
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		timeline ??= MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)], [MatchTimelineFactory.Economy(1, 4000, 4000)]) with
		{
			Positions = [OpponentTimelineFactory.Track(1, 1, MapSide.T, OpponentTimelineFactory.SiteA)]
		};
		await MatchTimelineAttacher.AttachAsync(
			dbContext, storage, match.Id, timeline, DemoTimelineSerializer.CurrentParserVersion, DemoTimelineFactory.TeamA, CancellationToken.None);
		return match;
	}

	#endregion

	#region Private Methods

	private static GetRoundReplayHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage) =>
		new(dbContext, storage, NullLogger<GetRoundReplayHandler>.Instance);

	#endregion
}
