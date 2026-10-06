#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Application.Features.OpponentReport.GetOpponentDemoReplay;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentDemoRows;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.GetOpponentDemoReplay;

public class GetOpponentDemoReplayHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage()).Handle(new GetOpponentDemoReplayQuery(Guid.NewGuid(), 1), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.NotFound.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_report_missing_storage_configuration()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var row = Row(Facts([]));
		dbContext.OpponentDemoAnalyses.Add(row);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, new TestFileStorage(isConfigured: false)).Handle(new GetOpponentDemoReplayQuery(row.Id, 1), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.StorageNotConfigured.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_report_an_unreadable_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var row = Row(Facts([]));
		dbContext.OpponentDemoAnalyses.Add(row);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, new TestFileStorage()).Handle(new GetOpponentDemoReplayQuery(row.Id, 1), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.TimelineUnavailable.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_replay_the_round_with_the_opponent_labelled_and_their_display_name()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		SeedLink(dbContext);
		var row = Row(Facts([]));
		dbContext.OpponentDemoAnalyses.Add(row);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage();
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T)], positions: [Track(1, Them[0], MapSide.T, SiteA)]);
		await MatchTimelineStorage.SaveAsync(storage, row.TimelineObjectKey, timeline, DemoTimelineSerializer.CurrentParserVersion, CancellationToken.None);

		var result = await Handler(dbContext, storage).Handle(new GetOpponentDemoReplayQuery(row.Id, 1), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Team X", result.Value.OpponentName);
		Assert.Null(result.Value.OurSide);
		Assert.All(result.Value.Players.Where(p => Them.Contains(long.Parse(p.SteamId64))), p => Assert.Equal(ReplayTeam.Opponent, p.Team));
		Assert.Single(result.Value.Frames[0].Players);
	}

	#endregion

	#region Private Methods

	private static GetOpponentDemoReplayHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage) =>
		new(dbContext, storage, NullLogger<GetOpponentDemoReplayHandler>.Instance);

	#endregion
}
