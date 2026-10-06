using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.Shared;

public class MatchBriefingFormatterTests
{
	#region Public Methods

	[Fact]
	public void Format_includes_top_three_insights_veto_players_and_link()
	{
		var report = Report(
			insights: [Insight("Info", "i4"), Insight("High", "i1"), Insight("Warning", "i2"), Insight("High", "i3"), Insight("Info", "i5")],
			players:
			[
				new MapPlayersToWatchDto("Mirage", [Player("a", "A", 1.1), Player("b", "B", 1.5)]),
				new MapPlayersToWatchDto("Nuke", [Player("b", "B", 1.3), Player("c", "C", 1.2), Player("d", "D", 0.9)])
			]);

		var text = MatchBriefingFormatter.Format("Mecz ligowy", "Team X", report, "https://hub.test/opponents/report?name=Team%20X");

		Assert.Contains("i1", text);
		Assert.Contains("i3", text);
		Assert.Contains("i2", text);
		Assert.DoesNotContain("i4", text);
		Assert.Contains("my ban Nuke → oni ban Mirage → decider Inferno", text);
		Assert.Contains("B (K/D 1.50, ADR 80), C (K/D 1.20, ADR 80), A (K/D 1.10, ADR 80)", text);
		Assert.EndsWith("https://hub.test/opponents/report?name=Team%20X", text);
	}

	[Fact]
	public void Format_stays_within_the_discord_limit_and_keeps_the_link()
	{
		var report = Report([Insight("High", new string('x', 3000))], []);

		var text = MatchBriefingFormatter.Format("Mecz", "Team X", report, "https://hub.test/r");

		Assert.True(text.Length <= MatchBriefingFormatter.MaxMessageLength);
		Assert.EndsWith("https://hub.test/r", text);
	}

	[Fact]
	public void Format_omits_the_link_without_a_base_url()
	{
		var text = MatchBriefingFormatter.Format("Mecz", "Team X", Report([Insight("High", "i1")], []), null);

		Assert.DoesNotContain("http", text);
	}

	#endregion

	#region Private Methods

	private static OpponentInsightDto Insight(string severity, string text) => new("K", severity, text, "e");

	private static PlayerToWatchDto Player(string id, string nick, double kd) => new(id, nick, 10, kd, 80, 50, 0.1);

	private static OpponentReportDto Report(List<OpponentInsightDto> insights, List<MapPlayersToWatchDto> players) =>
		new("Team X", true, null, DateTime.UtcNow, null, 0, 0, 0, 0, 0, insights, [],
			[new VetoPlanDto("Bo1",
			[
				new VetoPlanStepDto(1, "Us", "Ban", "Nuke", ""),
				new VetoPlanStepDto(2, "Opponent", "Ban", "Mirage", ""),
				new VetoPlanStepDto(3, "Us", "Decider", "Inferno", "")
			])],
			players, new OpponentFormDto([], null, []), null, null);

	#endregion
}
