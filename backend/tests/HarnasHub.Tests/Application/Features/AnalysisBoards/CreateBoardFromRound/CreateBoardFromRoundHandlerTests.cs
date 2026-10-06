#region Usings

using FluentValidation.TestHelper;
using HarnasHub.Application.Features.AnalysisBoards.CreateBoardFromRound;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis;
using HarnasHub.Tests.Application.Features.MatchAnalysis.GetRoundReplay;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.AnalysisBoards.CreateBoardFromRound;

public class CreateBoardFromRoundHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_save_the_round_snapshot_as_a_board_on_the_map()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var notifier = new TestRealtimeNotifier();
		var match = await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage);

		var result = await Handler(dbContext, storage, notifier).Handle(new CreateBoardFromRoundCommand(ReplaySource.Match, match.Id, 1, 12), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Runda 1 – 0:12 – vs Team X", result.Value.Title);
		var board = dbContext.AnalysisBoards.Single();
		Assert.Equal(MapName.Mirage, board.MapName);
		Assert.Null(board.BackgroundImageObjectKey);
		Assert.Contains("\"color\":\"#f59e0b\"", board.StrokesJson);
		Assert.Equal(["analysis-boards"], notifier.Topics);
	}

	[Fact]
	public async Task Should_refuse_a_map_without_a_verified_radar_fit()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var timeline = MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]) with { MapName = MapName.Inferno };
		var match = await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage, timeline);

		var result = await Handler(dbContext, storage).Handle(new CreateBoardFromRoundCommand(ReplaySource.Match, match.Id, 1, 5), CancellationToken.None);

		Assert.Equal(AnalysisBoardErrors.MapUncalibrated.Code, result.FirstError.Code);
		Assert.Empty(dbContext.AnalysisBoards);
	}

	[Fact]
	public async Task Should_refuse_a_timeline_without_positions()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var timeline = MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]);
		var match = await GetRoundReplayHandlerTests.SeedAsync(dbContext, storage, timeline);

		var result = await Handler(dbContext, storage).Handle(new CreateBoardFromRoundCommand(ReplaySource.Match, match.Id, 1, 5), CancellationToken.None);

		Assert.Equal(AnalysisBoardErrors.PositionsNotRecorded.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_opponent_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage()).Handle(
			new CreateBoardFromRoundCommand(ReplaySource.OpponentDemo, Guid.NewGuid(), 1, 5), CancellationToken.None);

		Assert.Equal("OpponentDemos.NotFound", result.FirstError.Code);
	}

	[Fact]
	public void Should_validate_round_second_and_source()
	{
		var validator = new CreateBoardFromRoundCommandValidator();

		var invalid = validator.TestValidate(new CreateBoardFromRoundCommand((ReplaySource)9, Guid.Empty, 0, -1));
		invalid.ShouldHaveValidationErrorFor(x => x.Source);
		invalid.ShouldHaveValidationErrorFor(x => x.SourceId);
		invalid.ShouldHaveValidationErrorFor(x => x.RoundNumber);
		invalid.ShouldHaveValidationErrorFor(x => x.Second);
		validator.TestValidate(new CreateBoardFromRoundCommand(ReplaySource.Match, Guid.NewGuid(), 1, 0)).ShouldNotHaveAnyValidationErrors();
	}

	#endregion

	#region Private Methods

	private static CreateBoardFromRoundHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage, TestRealtimeNotifier? notifier = null) =>
		new(dbContext, storage, new TestCurrentUserService(Guid.NewGuid()), notifier ?? new TestRealtimeNotifier(), NullLogger<CreateBoardFromRoundHandler>.Instance);

	#endregion
}
