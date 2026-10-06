#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class MapInsightRulesTests
{
	#region Public Methods

	[Fact]
	public void Should_return_nothing_without_analysed_matches()
	{
		Assert.Empty(MapInsightRules.Build(Data(matches: 0)));
	}

	[Fact]
	public void Should_flag_a_big_side_imbalance_as_negative()
	{
		var result = MapInsightRules.Build(Data(t: new WinRateDto(2, 10), ct: new WinRateDto(9, 10)));

		var insight = Assert.Single(result, i => i.Code == "side-imbalance");
		Assert.Equal(InsightTone.Negative, insight.Tone);
		Assert.Contains("T", insight.Title);
	}

	[Fact]
	public void Should_praise_a_strong_opening_zone_and_flag_a_weak_one()
	{
		var result = MapInsightRules.Build(Data(openings:
		[
			new MapOpeningZoneDto("T", "A Site", 7, 2),
			new MapOpeningZoneDto("CT", "Mid", 1, 5)
		]));

		Assert.Contains(result, i => i.Code == "opening-best" && i.Tone == InsightTone.Positive);
		Assert.Contains(result, i => i.Code == "opening-worst" && i.Tone == InsightTone.Negative);
	}

	[Fact]
	public void Should_stay_silent_below_the_minimum_sample()
	{
		var result = MapInsightRules.Build(Data(t: new WinRateDto(0, 3), ct: new WinRateDto(3, 3)));

		Assert.DoesNotContain(result, i => i.Code == "side-imbalance");
	}

	#endregion

	#region Private Methods

	private static MapAnalyticsDto Data(
		int matches = 3,
		WinRateDto? t = null,
		WinRateDto? ct = null,
		IReadOnlyList<MapOpeningZoneDto>? openings = null) => new(
		"Mirage", true, matches, 0, 20,
		t ?? new WinRateDto(5, 10), ct ?? new WinRateDto(5, 10), new WinRateDto(1, 2),
		[], [], [], openings ?? [], new MapTradesDto(0, 0, 0), [], []);

	#endregion
}
