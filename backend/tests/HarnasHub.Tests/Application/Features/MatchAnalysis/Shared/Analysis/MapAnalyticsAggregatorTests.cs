#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class MapAnalyticsAggregatorTests
{
	#region Public Methods

	[Fact]
	public void Should_sum_side_pistol_and_site_stats_over_matches()
	{
		var rounds = new[] { Round(1, MapSide.T, plantSite: DemoBombSite.A), Round(2, MapSide.CT) };
		var match = new LoadedMatchTimeline(Stored(Timeline(rounds, [Economy(1, 800, 800), Economy(2, 4000, 4000)])), DemoTimelineFactory.TeamA);

		var result = MapAnalyticsAggregator.Aggregate(MapName.Mirage, [match, match]);

		Assert.Equal(2, result.MatchesAnalyzed);
		Assert.Equal(4, result.TSide.Total);
		Assert.Equal(2, result.TSide.Won);
		Assert.Equal(new WinRateDto(2, 2), result.Pistol);
		var site = Assert.Single(result.TSites);
		Assert.Equal("A", site.Site);
		Assert.Equal(2, site.Total);
		Assert.Contains(result.BuyTypes, b => b.BuyType == "Full" && b.Total == 2);
	}

	[Fact]
	public void Should_skip_matches_without_a_resolved_team()
	{
		var match = new LoadedMatchTimeline(Stored(Timeline([Round(1, MapSide.T)])), []);

		var result = MapAnalyticsAggregator.Aggregate(MapName.Mirage, [match], alreadySkipped: 1);

		Assert.Equal(0, result.MatchesAnalyzed);
		Assert.Equal(2, result.MatchesSkipped);
		Assert.Empty(result.Insights);
	}

	[Fact]
	public void Should_collect_clutchers()
	{
		var timeline = Timeline([Round(1, MapSide.T)], kills: [Kill(1, 3, 1), Kill(1, 2, 3, seconds: 20), Kill(1, 2, 4, seconds: 30)]);
		var match = new LoadedMatchTimeline(Stored(timeline), DemoTimelineFactory.TeamA);

		var result = MapAnalyticsAggregator.Aggregate(MapName.Mirage, [match]);

		var clutcher = Assert.Single(result.TopClutchers);
		Assert.Equal(1, clutcher.Won);
	}

	#endregion
}
