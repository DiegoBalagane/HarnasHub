#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.AttachDemoToResult;

public class AttachDemoToResultHandlerTests
{
	#region Private Fields

	private const string DemoKey = "demos/0123456789abcdef0123456789abcdef";

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_the_timeline_and_delete_the_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result(ourScore: 1, opponentScore: 0);
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage();
		var parser = new TestDemoParser(timeline: MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]));

		var result = await Handler(dbContext, storage, parser).Handle(new AttachDemoToResultCommand(match.Id, DemoKey), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.OurTeamResolved);
		Assert.Equal(1, result.Value.RoundsCount);
		Assert.Contains(DemoKey, storage.DeletedKeys);
		Assert.True(storage.Objects.ContainsKey(MatchTimelineStorage.MatchKey(match.Id)));
		Assert.Equal(DemoParseOptions.MatchAnalysis, parser.LastOptions);

		var analysis = await dbContext.MatchDemoAnalyses.SingleAsync();
		Assert.Equal(DemoTimelineFactory.TeamA, analysis.OurTeamSteamIds);
		Assert.Equal(DemoTimelineSerializer.CurrentParserVersion, analysis.ParserVersion);
	}

	[Fact]
	public async Task Should_replace_an_existing_timeline_instead_of_adding_a_second_row()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result();
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage();
		var parser = new TestDemoParser(timeline: MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]));

		await Handler(dbContext, storage, parser).Handle(new AttachDemoToResultCommand(match.Id, DemoKey), CancellationToken.None);
		await Handler(dbContext, storage, parser).Handle(new AttachDemoToResultCommand(match.Id, DemoKey), CancellationToken.None);

		Assert.Equal(1, await dbContext.MatchDemoAnalyses.CountAsync());
	}

	[Fact]
	public async Task Should_reject_an_unreadable_demo_and_still_delete_it()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result();
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage();

		var result = await Handler(dbContext, storage, new TestDemoParser(throwOnParse: new InvalidDataException()))
			.Handle(new AttachDemoToResultCommand(match.Id, DemoKey), CancellationToken.None);

		Assert.Equal("Results.InvalidDemoFile", result.FirstError.Code);
		Assert.Contains(DemoKey, storage.DeletedKeys);
		Assert.Empty(dbContext.MatchDemoAnalyses);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_match()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();

		var result = await Handler(dbContext, storage, new TestDemoParser())
			.Handle(new AttachDemoToResultCommand(Guid.NewGuid(), DemoKey), CancellationToken.None);

		Assert.Equal("Results.MatchNotFound", result.FirstError.Code);
		Assert.Contains(DemoKey, storage.DeletedKeys);
	}

	[Fact]
	public async Task Should_report_a_save_failure()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var match = MatchTimelineFactory.Result();
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage { ThrowOnUpload = new IOException("bucket down") };
		var parser = new TestDemoParser(timeline: MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)]));

		var result = await Handler(dbContext, storage, parser).Handle(new AttachDemoToResultCommand(match.Id, DemoKey), CancellationToken.None);

		Assert.Equal("MatchAnalysis.TimelineSaveFailed", result.FirstError.Code);
	}

	[Theory]
	[InlineData(DemoKey, true)]
	[InlineData("matches/x/timeline.json.gz", false)]
	[InlineData("demos/../secret", false)]
	[InlineData("", false)]
	public void Should_only_accept_presigned_demo_keys(string key, bool valid)
	{
		var validation = new AttachDemoToResultCommandValidator().Validate(new AttachDemoToResultCommand(Guid.NewGuid(), key));

		Assert.Equal(valid, validation.IsValid);
	}

	#endregion

	#region Private Methods

	private static AttachDemoToResultHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage, TestDemoParser parser) =>
		new(dbContext, storage, parser, new TestRealtimeNotifier(), NullLogger<AttachDemoToResultHandler>.Instance);

	#endregion
}
