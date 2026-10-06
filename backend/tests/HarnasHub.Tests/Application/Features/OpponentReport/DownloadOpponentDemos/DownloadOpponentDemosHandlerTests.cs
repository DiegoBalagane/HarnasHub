#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.DownloadOpponentDemos;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.DownloadOpponentDemos;

public class DownloadOpponentDemosHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_download_and_analyse_the_newest_team_games_on_the_chosen_maps()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		SeedLink(dbContext);
		var newest = Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow.AddDays(-1), "m-new");
		var older = Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow.AddDays(-5), "m-old");
		var otherMap = Match("de_inferno", Them, Strangers(), 13, 7, DateTime.UtcNow.AddDays(-2), "m-inf");
		dbContext.FaceitMatches.AddRange(newest, older, otherMap);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var faceit = new TestFaceitClient();
		faceit.Matches["m-new"] = new FaceitMatchInfo("m-new", []) { DemoUrls = ["https://demos/new.dem.zst"] };
		faceit.Matches["m-old"] = new FaceitMatchInfo("m-old", []) { DemoUrls = ["https://demos/old.dem.zst"] };
		var downloader = new TestFaceitDemoDownloader();
		var storage = new TestFileStorage();

		var result = await Handler(dbContext, storage, faceit, downloader)
			.Handle(new DownloadOpponentDemosCommand("Team X", [MapName.Mirage], 1), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(1, result.Value.Analysed);
		Assert.Equal(new[] { "https://demos/new.dem.zst" }, downloader.Requested);
		var row = Assert.Single(dbContext.OpponentDemoAnalyses);
		Assert.Equal(OpponentDemoSource.FaceitDownload, row.Source);
		Assert.Equal("m-new", row.FaceitMatchId);
		Assert.Equal(newest.PlayedAtUtc, row.PlayedAtUtc);
	}

	[Fact]
	public async Task Should_count_failures_and_continue_with_the_rest()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		SeedLink(dbContext);
		dbContext.FaceitMatches.AddRange(
			Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow.AddDays(-1), "m1"),
			Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow.AddDays(-2), "m2"),
			Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow.AddDays(-3), "m3"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var faceit = new TestFaceitClient();
		faceit.Matches["m1"] = new FaceitMatchInfo("m1", []) { DemoUrls = ["u1"] };
		faceit.Matches["m2"] = new FaceitMatchInfo("m2", []);
		faceit.Matches["m3"] = new FaceitMatchInfo("m3", []) { DemoUrls = ["u3"] };
		var downloader = new TestFaceitDemoDownloader();
		downloader.FailingUrls.Add("u3");

		var result = await Handler(dbContext, new TestFileStorage(), faceit, downloader)
			.Handle(new DownloadOpponentDemosCommand("team x", null, 3), CancellationToken.None);

		Assert.Equal(3, result.Value.Candidates);
		Assert.Equal(1, result.Value.Analysed);
		Assert.Equal(2, result.Value.Failed);
	}

	[Fact]
	public async Task Should_require_the_downloads_token_and_a_link()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var noToken = await Handler(dbContext, new TestFileStorage(), new TestFaceitClient(), new TestFaceitDemoDownloader(isConfigured: false))
			.Handle(new DownloadOpponentDemosCommand("team x", null, 1), CancellationToken.None);
		var noLink = await Handler(dbContext, new TestFileStorage(), new TestFaceitClient(), new TestFaceitDemoDownloader())
			.Handle(new DownloadOpponentDemosCommand("team x", null, 1), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.DownloadsNotConfigured.Code, noToken.FirstError.Code);
		Assert.Equal(OpponentReportErrors.NotLinked.Code, noLink.FirstError.Code);
	}

	[Fact]
	public void Should_skip_solo_games_other_maps_and_already_analysed_ones()
	{
		var roster = Them.ToHashSet();
		var team = Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow, "team");
		var solo = Match("de_mirage", ["t1", "x2", "x3", "x4", "x5"], Strangers(), 13, 7, DateTime.UtcNow, "solo");
		var analysed = Match("de_mirage", Them, Strangers(), 13, 7, DateTime.UtcNow, "done");
		var nuke = Match("de_nuke", Them, Strangers(), 13, 7, DateTime.UtcNow, "nuke");

		var candidates = DownloadOpponentDemosHandler.SelectCandidates(
			[team, solo, analysed, nuke], roster, [MapName.Mirage], new HashSet<(string, int)> { ("done", 1) }, 5);

		Assert.Equal("team", Assert.Single(candidates).FaceitMatchId);
	}

	[Theory]
	[InlineData(0, false)]
	[InlineData(3, true)]
	[InlineData(6, false)]
	public void Should_limit_the_count(int count, bool valid)
	{
		Assert.Equal(valid, new DownloadOpponentDemosCommandValidator().Validate(new DownloadOpponentDemosCommand("team x", null, count)).IsValid);
	}

	#endregion

	#region Private Methods

	private static DownloadOpponentDemosHandler Handler(
		TestApplicationDbContext dbContext, TestFileStorage storage, TestFaceitClient faceit, TestFaceitDemoDownloader downloader) =>
		new(dbContext, storage, new TestDemoParser(timeline: OpponentTimelineFactory.Timeline([OpponentTimelineFactory.Round(1, MapSide.T, MapSide.T)])), faceit, downloader,
			new TestCurrentUserService(Guid.NewGuid()), new TestJobProgress(), TestTeamNotifications.Create(dbContext, storage), NullLogger<DownloadOpponentDemosHandler>.Instance);

	private static void SeedLink(TestApplicationDbContext dbContext)
	{
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink
		{
			Id = Guid.NewGuid(),
			OpponentKey = "team x",
			DisplayName = "Team X",
			PlayerIds = [.. FaceitTestData.Them],
			LinkedAtUtc = DateTime.UtcNow
		});
		dbContext.SaveChanges();
	}

	#endregion
}
