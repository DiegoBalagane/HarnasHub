#region Usings

using HarnasHub.Application.Features.OpponentReport.GetOpponentDemos;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentDemoRows;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.GetOpponentDemos;

public class GetOpponentDemosHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_list_only_the_opponents_demos_and_offer_auto_download_when_everything_is_configured()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		OpponentDemoRows.SeedLink(dbContext);
		dbContext.OpponentDemoAnalyses.AddRange(Row(), Row(opponentKey: "someone else"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentDemosHandler(dbContext, new TestFileStorage(), new TestFaceitClient(), new TestFaceitDemoDownloader())
			.Handle(new GetOpponentDemosQuery("TEAM X"), CancellationToken.None);

		Assert.True(result.Value.StorageConfigured);
		Assert.True(result.Value.AutoDownloadAvailable);
		var demo = Assert.Single(result.Value.Demos);
		Assert.False(demo.TeamResolved);
		Assert.Equal(5, demo.Teams[0].Names.Count);
	}

	[Theory]
	[InlineData(false, true, true)]
	[InlineData(true, false, true)]
	[InlineData(true, true, false)]
	public async Task Should_not_offer_auto_download_without_a_token_api_key_or_link(bool downloader, bool faceit, bool linked)
	{
		await using var dbContext = TestApplicationDbContext.Create();
		if (linked)
		{
			OpponentDemoRows.SeedLink(dbContext);
		}

		var result = await new GetOpponentDemosHandler(
				dbContext, new TestFileStorage(), new TestFaceitClient(faceit), new TestFaceitDemoDownloader(downloader))
			.Handle(new GetOpponentDemosQuery("team x"), CancellationToken.None);

		Assert.False(result.Value.AutoDownloadAvailable);
		Assert.Empty(result.Value.Demos);
	}

	[Fact]
	public void Should_require_a_name()
	{
		Assert.False(new GetOpponentDemosQueryValidator().Validate(new GetOpponentDemosQuery(" ")).IsValid);
	}

	#endregion
}
