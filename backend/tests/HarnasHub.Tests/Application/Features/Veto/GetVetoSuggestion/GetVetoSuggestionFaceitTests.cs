using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.Veto.GetVetoSuggestion;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.GetVetoSuggestion;

public class GetVetoSuggestionFaceitTests
{
	#region Public Methods

	[Fact]
	public async Task Should_feed_the_faceit_advantage_from_the_report_snapshot_into_the_score()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetVetoSuggestionHandler(dbContext);
		var before = await handler.Handle(new GetVetoSuggestionQuery("Team X"), CancellationToken.None);

		var report = OpponentReportBuilder.Build(new OpponentReportInput(
			"Team X", null, DateTime.UtcNow, [], [], new HashSet<string>(), new HashSet<string>(), []));
		var maps = report.Maps
			.Select(m => m.MapName == "Mirage" ? m with { TheirGames = 12, OurGames = 12, Advantage = 25 } : m)
			.ToList();
		await OpponentReportSnapshots.SaveAsync(dbContext, "team x", report with { Maps = maps }, CancellationToken.None);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var after = await handler.Handle(new GetVetoSuggestionQuery("Team X"), CancellationToken.None);

		var mirageBefore = before.Value.Maps.Single(m => m.MapName == "Mirage");
		var mirageAfter = after.Value.Maps.Single(m => m.MapName == "Mirage");
		Assert.Equal(mirageBefore.Score + 20, mirageAfter.Score);
		Assert.Contains(mirageAfter.Reasons, r => r.StartsWith("FACEIT: przewaga +25 pp"));
		Assert.Equal(
			before.Value.Maps.Single(m => m.MapName == "Nuke").Score,
			after.Value.Maps.Single(m => m.MapName == "Nuke").Score);
	}

	#endregion
}
