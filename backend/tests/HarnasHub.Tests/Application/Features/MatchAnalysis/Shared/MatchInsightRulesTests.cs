#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class MatchInsightRulesTests
{
	#region Public Methods

	[Fact]
	public void Should_explain_itself_when_our_team_is_unknown()
	{
		var dto = MatchTimelineMapper.Map(MatchTimelineFactory.Stored(MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.T)])), []);

		var insight = Assert.Single(MatchInsightRules.Build(dto));
		Assert.Equal("team-unknown", insight.Code);
	}

	[Fact]
	public void Should_call_out_lost_force_buys()
	{
		// Team A (T) forces in rounds 2-4 against full buys and loses all of them.
		var rounds = new[] { 2, 3, 4 }.Select(n => MatchTimelineFactory.Round(n, MapSide.CT)).ToList();
		var economy = new[] { 2, 3, 4 }.Select(n => MatchTimelineFactory.Economy(n, 3000, 4500)).ToList();

		var insights = Build(rounds, economy);

		var force = Assert.Single(insights, i => i.Code == "buy-force");
		Assert.Equal(InsightTone.Negative, force.Tone);
		Assert.StartsWith("0/3", force.Title);
	}

	[Fact]
	public void Should_summarise_pistol_rounds_with_their_follow_ups()
	{
		var rounds = new List<DemoTimelineRound>
		{
			MatchTimelineFactory.Round(1, MapSide.T),
			MatchTimelineFactory.Round(2, MapSide.T),
			MatchTimelineFactory.Round(3, MapSide.T),
			MatchTimelineFactory.Round(13, MapSide.T, teamAOnT: false)
		};

		var pistols = Assert.Single(Build(rounds), i => i.Code == "pistols");

		Assert.Equal("Pistolówki: 1/2", pistols.Title);
		Assert.Contains("R1 (T): wygrana, potem 2/2 wygranych", pistols.Detail);
		Assert.Equal(InsightTone.Neutral, pistols.Tone);
	}

	[Fact]
	public void Should_flag_the_zone_where_most_opening_duels_are_lost()
	{
		// Team A on CT (teamAOnT false) loses three openings in Mirage B site: victim (ours) stood on B.
		var bSite = new DemoPosition(0, 0, 0, 225f / 1374, 242f / 1196);
		var rounds = new[] { 13, 14, 15 }.Select(n => MatchTimelineFactory.Round(n, MapSide.T, teamAOnT: false)).ToList();
		var kills = rounds.Select(r => MatchTimelineFactory.Kill(r.Number, 3, 1, isOpening: true, teamAOnT: false, victimPosition: bSite)).ToList();

		var insight = Assert.Single(Build(rounds, kills: kills), i => i.Code == "opening-zone");

		Assert.Equal(InsightTone.Negative, insight.Tone);
		Assert.Equal("100% otwarć przegranych na CT B", insight.Title);
	}

	[Fact]
	public void Should_report_post_plant_conversion_and_retakes()
	{
		var rounds = new List<DemoTimelineRound>
		{
			MatchTimelineFactory.Round(2, MapSide.T, plantSite: DemoBombSite.A),
			MatchTimelineFactory.Round(3, MapSide.T, plantSite: DemoBombSite.B),
			MatchTimelineFactory.Round(14, MapSide.T, teamAOnT: false, plantSite: DemoBombSite.A),
			MatchTimelineFactory.Round(15, MapSide.CT, teamAOnT: false, plantSite: DemoBombSite.A, defused: true)
		};

		var insights = Build(rounds);

		Assert.Equal("Po plancie: 2/2", Assert.Single(insights, i => i.Code == "post-plant").Title);
		Assert.Equal("Retake'i: 1/2", Assert.Single(insights, i => i.Code == "retakes").Title);
	}

	[Fact]
	public void Should_flag_lost_anti_eco_rounds()
	{
		var rounds = new[] { MatchTimelineFactory.Round(5, MapSide.CT), MatchTimelineFactory.Round(6, MapSide.T) };
		var economy = new[] { MatchTimelineFactory.Economy(5, 4500, 600), MatchTimelineFactory.Economy(6, 4500, 600) };

		var insight = Assert.Single(Build(rounds, economy), i => i.Code == "anti-eco");

		Assert.Equal(InsightTone.Negative, insight.Tone);
		Assert.Contains("R5", insight.Detail);
	}

	[Fact]
	public void Should_never_return_more_than_the_cap_and_put_problems_first()
	{
		var rounds = Enumerable.Range(2, 10).Select(n => MatchTimelineFactory.Round(n, n % 2 == 0 ? MapSide.T : MapSide.CT, plantSite: DemoBombSite.A)).ToList();
		var economy = Enumerable.Range(2, 10).Select(n => MatchTimelineFactory.Economy(n, n % 3 == 0 ? 3000 : 4500, n % 4 == 0 ? 600 : 4500)).ToList();

		var insights = Build(rounds, economy);

		Assert.True(insights.Count <= MatchInsightRules.MaxInsights);
		var tones = insights.Select(i => i.Tone == InsightTone.Negative ? 0 : i.Tone == InsightTone.Positive ? 1 : 2).ToList();
		Assert.Equal(tones.OrderBy(t => t), tones);
	}

	#endregion

	#region Private Methods

	private static IReadOnlyList<MatchInsightDto> Build(
		IReadOnlyList<DemoTimelineRound> rounds,
		IReadOnlyList<DemoRoundEconomy>? economy = null,
		IReadOnlyList<DemoKill>? kills = null)
	{
		var timeline = MatchTimelineFactory.Timeline(rounds, economy, kills);
		return MatchInsightRules.Build(MatchTimelineMapper.Map(MatchTimelineFactory.Stored(timeline), DemoTimelineFactory.TeamA));
	}

	#endregion
}
