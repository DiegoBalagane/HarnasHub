using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class DemoReviewFormatterTests
{
	#region Public Methods

	[Fact]
	public void Format_lists_at_most_five_insights_with_header_and_link()
	{
		var insights = Enumerable.Range(1, 6).Select(i => new MatchInsightDto($"c{i}", InsightTone.Negative, $"Tytuł {i}", $"Opis {i}", 5)).ToList();

		var text = DemoReviewFormatter.Format("Team X", 13, 7, "Mirage", insights, "https://hub.test/results/1");

		Assert.StartsWith("🎬 **Analiza demki: vs Team X** 13:7 (Mirage)", text);
		Assert.Contains("🔴 **Tytuł 5** — Opis 5", text);
		Assert.DoesNotContain("Tytuł 6", text);
		Assert.EndsWith("🔗 Pełna analiza: https://hub.test/results/1", text);
	}

	[Fact]
	public void Format_marks_tones_with_icons()
	{
		var text = DemoReviewFormatter.Format("X", 1, 0, null,
			[new("a", InsightTone.Positive, "Plus", "d", 3), new("b", InsightTone.Neutral, "Neutral", "d", 3)], null);

		Assert.Contains("🟢 **Plus**", text);
		Assert.Contains("⚪ **Neutral**", text);
		Assert.DoesNotContain("🔗", text);
	}

	[Fact]
	public void Format_skips_the_team_unknown_placeholder()
	{
		var text = DemoReviewFormatter.Format("X", 1, 0, null,
			[new("team-unknown", InsightTone.Neutral, "Nie rozpoznano naszej drużyny", "d", 0)], null);

		Assert.DoesNotContain("Nie rozpoznano", text);
		Assert.Contains("za mało danych", text);
	}

	[Fact]
	public void Format_stays_within_the_limit_and_keeps_the_link()
	{
		var text = DemoReviewFormatter.Format("X", 1, 0, null,
			[new("a", InsightTone.Negative, "T", new string('d', 4000), 3)], "https://hub.test/results/1");

		Assert.True(text.Length <= 2000);
		Assert.EndsWith("https://hub.test/results/1", text);
	}

	#endregion
}
