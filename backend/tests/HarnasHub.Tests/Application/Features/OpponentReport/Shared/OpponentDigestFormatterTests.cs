using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OpponentDigestFormatterTests
{
	#region Public Methods

	[Fact]
	public void Format_groups_suggestions_by_map_orders_by_confidence_and_caps_at_five()
	{
		var tendencies = new List<MapTendenciesDto>
		{
			Map("Mirage", 3, "Low", "m-low", "High", "m-high", "Medium", "m-med"),
			Map("Nuke", 1, "High", "n1", "High", "n2", "High", "n3")
		};

		var text = OpponentDigestFormatter.Format("Team X", ["Mirage"], tendencies, "https://hub.test/opponents/report?name=Team%20X");

		Assert.StartsWith("🕵️ **Nowa demka przeciwnika: Team X** (Mirage)", text);
		Assert.True(text.IndexOf("m-high", StringComparison.Ordinal) < text.IndexOf("m-med", StringComparison.Ordinal));
		Assert.True(text.IndexOf("m-med", StringComparison.Ordinal) < text.IndexOf("m-low", StringComparison.Ordinal));
		Assert.Contains("**Nuke** (z 1 demek):", text);
		Assert.Contains("n2", text);
		Assert.DoesNotContain("n3", text);
		Assert.Equal(5, text.Split('\n').Count(l => l.StartsWith("• ")));
		Assert.EndsWith("🔗 Raport przeciwnika: https://hub.test/opponents/report?name=Team%20X", text);
	}

	[Fact]
	public void Format_without_suggestions_or_link_says_there_is_not_enough_data()
	{
		var text = OpponentDigestFormatter.Format("Team X", [], [], null);

		Assert.Contains("Za mało danych", text);
		Assert.DoesNotContain("🔗", text);
	}

	#endregion

	#region Private Methods

	private static MapTendenciesDto Map(string name, int demos, params string[] confidenceAndText)
	{
		var suggestions = new List<AntiStratSuggestionDto>();
		for (var i = 0; i < confidenceAndText.Length; i += 2)
		{
			suggestions.Add(new AntiStratSuggestionDto("k", "T", confidenceAndText[i + 1], "e", confidenceAndText[i]));
		}

		return new MapTendenciesDto(name, demos, 20, true, null, null, null!, null!, [], suggestions);
	}

	#endregion
}
