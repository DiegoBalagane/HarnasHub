using HarnasHub.Application.Features.OpponentReport.GetOpponentReport;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.GetOpponentReport;

public class GetOpponentReportHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_build_a_live_report_from_the_cache_when_there_is_no_snapshot()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink
		{
			Id = Guid.NewGuid(),
			OpponentKey = "team x",
			DisplayName = "Team X",
			PlayerIds = [.. Them],
			LinkedAtUtc = DateTime.UtcNow
		});
		dbContext.FaceitMatches.Add(Match("de_mirage", Them, Strangers(), 13, 4, DateTime.UtcNow.AddDays(-3)));
		dbContext.FaceitMatches.Add(Match("de_mirage", Them, Strangers(), 13, 4, DateTime.UtcNow.AddDays(-200)));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentReportHandler(dbContext, new TestFaceitClient(isConfigured: false))
			.Handle(new GetOpponentReportQuery(" TEAM X "), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Team X", result.Value.OpponentName);
		Assert.NotNull(result.Value.Link);
		Assert.Equal(1, result.Value.TheirTeamGames);
		Assert.False(result.Value.FaceitConfigured);
		Assert.Null(result.Value.NextEventId);
		Assert.Empty(dbContext.OpponentReportSnapshots);
	}

	[Fact]
	public async Task Should_return_the_stored_snapshot_with_live_fields_refreshed()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var stored = OpponentReportBuilder.Build(new OpponentReportInput(
			"Team X", null, DateTime.UtcNow.AddHours(-5), [], [], new HashSet<string>(), new HashSet<string>(), [])) with
		{ TheirTeamGames = 42 };
		await OpponentReportSnapshots.SaveAsync(dbContext, "team x", stored, CancellationToken.None);
		var nextEvent = new Event { Id = Guid.NewGuid(), Title = "Mecz", Opponent = "Team X ", StartsAtUtc = DateTime.UtcNow.AddDays(1) };
		dbContext.Events.Add(nextEvent);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetOpponentReportHandler(dbContext, new TestFaceitClient())
			.Handle(new GetOpponentReportQuery("team x"), CancellationToken.None);

		Assert.Equal(42, result.Value.TheirTeamGames);
		Assert.True(result.Value.FaceitConfigured);
		Assert.Equal(nextEvent.Id, result.Value.NextEventId);
	}

	#endregion
}
