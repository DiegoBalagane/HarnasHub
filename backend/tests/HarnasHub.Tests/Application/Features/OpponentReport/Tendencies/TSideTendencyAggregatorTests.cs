#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class TSideTendencyAggregatorTests
{
	#region Public Methods

	[Fact]
	public void Should_aggregate_targets_and_execute_timing()
	{
		var rounds = new[]
		{
			TRound(2, MapArea.B, 80f),
			TRound(3, MapArea.B, 90f),
			TRound(4, MapArea.A, 20f),
			TRound(5, null, null)
		}.Select(r => new IndexedRound(0, r)).ToList();

		var t = TSideTendencyAggregator.Aggregate(MapName.Mirage, rounds);

		Assert.Equal(4, t.Rounds);
		Assert.Equal("Low", t.Confidence);
		Assert.Equal("B", t.Targets[0].Label);
		Assert.Equal(66.7, t.Targets[0].Percent);
		Assert.Equal(2, t.ExecTiming.Single(s => s.Label == "Late").Count);
		Assert.Equal(1, t.ExecTiming.Single(s => s.Label == "Fast").Count);
		Assert.Equal(63.3, t.AverageExecSecond);
		Assert.Equal(2, t.Entries.Count);
	}

	[Fact]
	public void Should_report_grenade_clusters_as_a_share_of_t_rounds()
	{
		var smoke = new GrenadeFact(DemoGrenadeType.Smoke, SiteB.X, SiteB.Y, 30f);
		var rounds = new[]
		{
			new IndexedRound(0, TRound(2, MapArea.B, 70f, grenades: smoke)),
			new IndexedRound(1, TRound(2, MapArea.B, 70f, grenades: smoke)),
			new IndexedRound(1, TRound(3, MapArea.A, 70f))
		};

		var cluster = Assert.Single(TSideTendencyAggregator.Aggregate(MapName.Mirage, rounds).GrenadeClusters);

		Assert.Equal("Smoke", cluster.Type);
		Assert.Equal(2, cluster.Rounds);
		Assert.Equal(66.7, cluster.PerRoundPercent);
		Assert.Equal("B", cluster.Area);
	}

	[Fact]
	public void Should_summarise_pistols_and_the_buy_after_a_lost_one()
	{
		var rounds = new[]
		{
			new IndexedRound(0, TRound(1, MapArea.A, 40f, won: false, buy: "Pistol")),
			new IndexedRound(0, TRound(2, MapArea.A, 40f, buy: "Force")),
			new IndexedRound(0, CtRound(13, [MapArea.A], won: true)),
			new IndexedRound(1, TRound(1, MapArea.B, 40f, won: false, buy: "Pistol")),
			new IndexedRound(1, TRound(2, null, null, buy: "Eco"))
		};

		var pistol = TSideTendencyAggregator.Aggregate(MapName.Mirage, rounds).Pistol;

		Assert.Equal(3, pistol.PistolRounds);
		Assert.Equal(1, pistol.PistolWins);
		Assert.Equal(2, pistol.LostPistols);
		Assert.Equal(2, pistol.TPistolTargets.Count);
		Assert.Equal(new[] { "Eco", "Force" }, pistol.AfterLostPistolBuys.Select(b => b.Label).OrderBy(l => l));
	}

	[Theory]
	[InlineData(10f, "Fast")]
	[InlineData(50f, "Mid")]
	[InlineData(80f, "Late")]
	public void Should_bucket_execute_timing(float second, string expected)
	{
		Assert.Equal(expected, TSideTendencyAggregator.Timing(second));
	}

	#endregion
}
