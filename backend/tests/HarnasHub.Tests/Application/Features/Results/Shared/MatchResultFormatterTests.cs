using HarnasHub.Application.Features.Results.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.Shared;

public class MatchResultFormatterTests
{
	#region Public Methods

	[Fact]
	public void Format_shows_opponent_score_verdict_map_and_link()
	{
		var text = MatchResultFormatter.Format("Team X", 13, 7, "Mirage", "https://hub.test/results/1");

		Assert.Equal("🏆 Wynik meczu vs **Team X**: **13:7** (wygrana) — Mirage\n🔗 Szczegóły meczu: https://hub.test/results/1", text);
	}

	[Theory]
	[InlineData(5, 13, "porażka")]
	[InlineData(12, 12, "remis")]
	public void Format_names_the_verdict(int ours, int theirs, string verdict)
	{
		Assert.Contains($"({verdict})", MatchResultFormatter.Format("X", ours, theirs, null, null));
	}

	[Fact]
	public void Format_omits_map_and_link_when_missing()
	{
		var text = MatchResultFormatter.Format("X", 13, 0, " ", "");

		Assert.DoesNotContain("🔗", text);
		Assert.DoesNotContain("—", text);
	}

	#endregion
}
