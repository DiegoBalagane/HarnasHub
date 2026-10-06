using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class FaceitHistoryPagerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_page_through_history_and_keep_every_league_game_behind_the_latest_pugs()
	{
		var client = new TestFaceitClient();
		// 150 PUGs newest first, then 20 ESEA League games older than all of them.
		client.Histories["p1"] = Enumerable.Range(0, 150).Select(i => Item($"mm{i}", "matchmaking"))
			.Concat(Enumerable.Range(0, 20).Select(i => Item($"esea{i}", "championship")))
			.ToList();

		var history = await FaceitHistoryPager.LoadAsync(client, "p1", DateTime.UtcNow.AddDays(-120), CancellationToken.None);
		var selected = FaceitHistoryPager.SelectToFetch(history).ToList();

		Assert.Equal(170, history.Count);
		Assert.Equal(2, client.HistoryRequests.Count);
		Assert.Equal(20 + FaceitSync.HistoryLimit, selected.Count);
		Assert.All(selected.Take(20), h => Assert.Equal("championship", h.CompetitionType));
	}

	#endregion

	#region Private Methods

	private static FaceitHistoryItem Item(string id, string type) =>
		new(id, DateTime.UtcNow, type, type == "championship" ? "ESEA League" : "5v5 Queue", "FINISHED");

	#endregion
}
