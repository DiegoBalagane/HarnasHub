#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class AntiStratRulesTests
{
	#region Public Methods

	[Fact]
	public void Should_suggest_a_stack_when_most_t_rounds_go_late_to_one_site()
	{
		var rounds = Enumerable.Range(2, 7).Select(n => TRound(n, MapArea.B, 90f + n)).Append(TRound(9, MapArea.A, 30f));

		var suggestions = Evaluate(Facts(rounds));

		var target = suggestions.Single(s => s.Kind == "TTarget");
		Assert.Contains("88% rund T idzie na B", target.Text);
		Assert.Contains("rozważ stack B", target.Text);
		Assert.Contains(suggestions, s => s.Kind == "TSlow");
	}

	[Fact]
	public void Should_flag_fast_rounds_and_standard_smokes()
	{
		var smoke = new GrenadeFact(DemoGrenadeType.Smoke, SiteA.X, SiteA.Y, 10f);
		var rounds = Enumerable.Range(2, 6).Select(n => TRound(n, n % 2 == 0 ? MapArea.A : MapArea.B, 20f, grenades: smoke));

		var suggestions = Evaluate(Facts(rounds));

		Assert.Contains(suggestions, s => s.Kind == "TFast");
		Assert.Contains(suggestions, s => s.Kind == "TStandardSmoke" && s.Text.Contains("strefa A"));
		Assert.DoesNotContain(suggestions, s => s.Kind == "TTarget");
	}

	[Fact]
	public void Should_describe_the_default_ct_setup_stacks_aggression_and_saves()
	{
		MapArea?[] stackA = [MapArea.A, MapArea.A, MapArea.A, MapArea.Mid, MapArea.B];
		var rounds = Enumerable.Range(13, 6).Select(n => CtRound(n, stackA, earlyKills: 1, postPlant: PostPlantBehaviour.Save, won: false));

		var suggestions = Evaluate(Facts(rounds));

		Assert.Contains(suggestions, s => s.Kind == "CtDefaultSetup" && s.Text.Contains("3A-1M-1B") && s.Text.Contains("słabiej obstawione B"));
		Assert.Contains(suggestions, s => s.Kind == "CtStack" && s.Text.Contains("Stackują A"));
		Assert.Contains(suggestions, s => s.Kind == "CtAggression");
		Assert.Contains(suggestions, s => s.Kind == "CtSaves");
	}

	[Fact]
	public void Should_stay_silent_on_a_small_sample()
	{
		var rounds = Enumerable.Range(2, 3).Select(n => TRound(n, MapArea.B, 80f));

		Assert.Empty(Evaluate(Facts(rounds)));
	}

	[Fact]
	public void Should_name_the_awper_and_the_entry()
	{
		var players = new[]
		{
			new OpponentPlayerFacts(11, "sniper", 20, 30, 2, 2, 12, 0, 0),
			new OpponentPlayerFacts(12, "entry", 20, 20, 6, 4, 0, 0, 0)
		};

		var suggestions = Evaluate(Facts([], players));

		Assert.Contains(suggestions, s => s.Kind == "PlayerAwp" && s.Text.StartsWith("sniper"));
		Assert.Contains(suggestions, s => s.Kind == "PlayerEntry" && s.Text.StartsWith("entry"));
	}

	[Theory]
	[InlineData(70, "1:10")]
	[InlineData(5, "0:05")]
	public void Should_format_round_time(double seconds, string expected)
	{
		Assert.Equal(expected, AntiStratRules.Clock(seconds));
	}

	#endregion

	#region Private Methods

	private static List<AntiStratSuggestionDto> Evaluate(OpponentDemoFacts facts) =>
		OpponentTendencyAggregator.Aggregate(MapName.Mirage, [facts]).Suggestions;

	#endregion
}
