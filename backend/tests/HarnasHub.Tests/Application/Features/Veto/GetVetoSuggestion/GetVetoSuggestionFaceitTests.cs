using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.Veto.GetVetoSuggestion;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.GetVetoSuggestion;

public class GetVetoSuggestionFaceitTests
{
	#region Public Methods

	[Fact]
	public async Task Should_feed_the_opponent_strength_from_the_report_snapshot_into_the_score()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetVetoSuggestionHandler(dbContext);
		var before = await handler.Handle(new GetVetoSuggestionQuery("Team X"), CancellationToken.None);

		var report = OpponentReportBuilder.Build(new OpponentReportInput(
			"Team X", null, DateTime.UtcNow, [], [], new HashSet<string>(), new HashSet<string>(), []));
		var maps = report.Maps
			.Select(m => m.MapName switch
			{
				"Mirage" => m with { TheirGames = 10, TheirWins = 2, TheirSmoothedWinRate = 30 },
				// An older snapshot row: no smoothed rate and no FACEIT wins of ours — falls back, our FACEIT games are skipped.
				"Nuke" => m with { TheirGames = 3, TheirWins = 3, OurFaceitGames = 4, OurFaceitWins = null, TheirSmoothedWinRate = null },
				_ => m
			})
			.ToList();
		await OpponentReportSnapshots.SaveAsync(dbContext, "team x", report with { Maps = maps }, CancellationToken.None);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var after = await handler.Handle(new GetVetoSuggestionQuery("Team X"), CancellationToken.None);

		var mirageBefore = before.Value.Maps.Single(m => m.MapName == "Mirage");
		var mirageAfter = after.Value.Maps.Single(m => m.MapName == "Mirage");
		Assert.Equal(mirageBefore.Score + 12, mirageAfter.Score);
		Assert.Contains("Słabość rywala: ich 20% wygranych przy 10 meczach", mirageAfter.Reasons);
		var nukeAfter = after.Value.Maps.Single(m => m.MapName == "Nuke");
		Assert.Equal(before.Value.Maps.Single(m => m.MapName == "Nuke").Score, nukeAfter.Score);
		Assert.Contains("Brak naszych meczów na tej mapie", nukeAfter.Reasons);
	}

	#endregion
}
