#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.AnalyzeOpponentDemo;

public class AnalyzeOpponentDemoHandlerTests
{
	#region Private Fields

	private const string DemoKey = "demos/0123456789abcdef0123456789abcdef";

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_analysis_with_positions_enabled_and_delete_the_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		OpponentDemoRows.SeedLink(dbContext);
		var storage = new TestFileStorage();
		var parser = new TestDemoParser(timeline: Timeline([Round(1, MapSide.T, MapSide.T)]));

		var result = await Handler(dbContext, storage, parser).Handle(new AnalyzeOpponentDemoCommand(" Team X ", DemoKey), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.TeamResolved);
		Assert.Equal("Upload", result.Value.Source);
		Assert.True(parser.LastOptions!.Includes(DemoCollectors.Positions));
		Assert.Contains(DemoKey, storage.DeletedKeys);
		var row = Assert.Single(dbContext.OpponentDemoAnalyses);
		Assert.Equal("team x", row.OpponentKey);
		Assert.StartsWith("opponents/team-x/", row.TimelineObjectKey);
	}

	[Fact]
	public async Task Should_reject_an_unreadable_demo_and_still_delete_it()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var parser = new TestDemoParser(throwOnParse: new InvalidDataException("bad"));

		var result = await Handler(dbContext, storage, parser).Handle(new AnalyzeOpponentDemoCommand("Team X", DemoKey), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.InvalidDemoFile.Code, result.FirstError.Code);
		Assert.Contains(DemoKey, storage.DeletedKeys);
		Assert.Empty(dbContext.OpponentDemoAnalyses);
	}

	[Fact]
	public async Task Should_reject_a_demo_without_rounds()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage(), new TestDemoParser(timeline: Timeline([])))
			.Handle(new AnalyzeOpponentDemoCommand("Team X", DemoKey), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.InvalidDemoFile.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_fail_without_storage()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage(isConfigured: false), new TestDemoParser())
			.Handle(new AnalyzeOpponentDemoCommand("Team X", DemoKey), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.StorageNotConfigured.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_report_a_timeline_upload_failure()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage { ThrowOnUpload = new IOException("down") };

		var result = await Handler(dbContext, storage, new TestDemoParser(timeline: Timeline([Round(1, MapSide.T, null)])))
			.Handle(new AnalyzeOpponentDemoCommand("Team X", DemoKey), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.TimelineUnavailable.Code, result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static AnalyzeOpponentDemoHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage, IDemoParser parser) =>
		new(dbContext, storage, parser, new TestCurrentUserService(Guid.NewGuid()), TestFaceitLookup.Create(dbContext), TestTeamNotifications.Create(dbContext, storage), NullLogger<AnalyzeOpponentDemoHandler>.Instance);

	#endregion
}
