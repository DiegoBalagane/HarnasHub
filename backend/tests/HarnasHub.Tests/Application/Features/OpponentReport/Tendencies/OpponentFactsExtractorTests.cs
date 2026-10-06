#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class OpponentFactsExtractorTests
{
	#region Public Methods

	[Fact]
	public void Should_extract_t_round_target_timing_contact_and_execute_grenades()
	{
		var timeline = Timeline(
			[Round(2, MapSide.T, MapSide.T, Plant(DemoBombSite.B, 70f))],
			kills: [Kill(2, 21, 11, 40f, killerAt: SiteB, victimAt: SiteB)],
			grenades:
			[
				Grenade(1, 2, 12, SiteB, seconds: 60f),
				Grenade(2, 2, 12, SiteA, seconds: 90f),
				Grenade(3, 2, 21, SiteB, seconds: 60f),
				Grenade(4, 2, 13, SiteB, seconds: 60f, type: DemoGrenadeType.Decoy)
			],
			economy: [Economy(2, 4500)]);

		var round = Assert.Single(OpponentFactsExtractor.Extract(timeline, Them.ToHashSet()).Rounds);

		Assert.Equal(MapSide.T, round.Side);
		Assert.True(round.Won);
		Assert.Equal("Full", round.Buy);
		Assert.NotNull(round.T);
		Assert.Null(round.Ct);
		Assert.Equal(MapArea.B, round.T.PlantArea);
		Assert.Equal(MapArea.B, round.T.FirstContactArea);
		Assert.Equal(40f, round.T.FirstContactSecond);
		Assert.Equal(70f, round.T.ExecSecond);
		Assert.Equal(MapArea.B, round.T.Target);
		var grenade = Assert.Single(round.T.Grenades);
		Assert.Equal(DemoGrenadeType.Smoke, grenade.Type);
	}

	[Fact]
	public void Should_extract_ct_setup_awp_kills_and_early_aggression()
	{
		var timeline = Timeline(
			[Round(14, MapSide.CT, MapSide.CT)],
			kills:
			[
				Kill(14, 11, 21, 10f, killerAt: Mid, victimAt: Mid, weapon: "awp", theirSide: MapSide.CT),
				Kill(14, 22, 12, 50f, killerAt: SiteA, victimAt: SiteA, theirSide: MapSide.CT)
			],
			positions:
			[
				Track(14, 11, MapSide.CT, Mid, "awp"),
				Track(14, 12, MapSide.CT, SiteA),
				Track(14, 13, MapSide.CT, SiteA),
				Track(14, 14, MapSide.CT, SiteB),
				Track(14, 15, MapSide.CT, SiteB),
				Track(14, 21, MapSide.T, SiteB)
			],
			economy: [Economy(14, 4500, MapSide.CT)]);

		var ct = Assert.Single(OpponentFactsExtractor.Extract(timeline, Them.ToHashSet()).Rounds).Ct;

		Assert.NotNull(ct);
		Assert.Equal(5, ct.Setup.Count);
		Assert.Equal("2A-1M-2B", CtSetupLabeler.Label(ct.Setup.Select(s => s.Area)));
		Assert.Single(ct.Setup, s => s.Awp);
		var awp = Assert.Single(ct.AwpKills);
		Assert.Equal(MapArea.Mid, awp.Area);
		Assert.Equal(1, ct.EarlyKills);
		Assert.Equal(0, ct.EarlyDeaths);
		Assert.Null(ct.PostPlant);
	}

	[Fact]
	public void Should_skip_rounds_the_opponent_did_not_play()
	{
		var round = new DemoTimelineRound(1, 0f, 15f, 100f, MapSide.T, DemoRoundEndReason.Elimination, [1, 2], [3, 4], null, null);

		Assert.Empty(OpponentFactsExtractor.Extract(Timeline([round]), Them.ToHashSet()).Rounds);
	}

	[Theory]
	[InlineData(1, 4000, "Pistol")]
	[InlineData(2, 500, "Eco")]
	[InlineData(2, 3000, "Force")]
	public void Should_classify_their_buy(int round, int perPlayer, string expected)
	{
		Assert.Equal(expected, OpponentFactsExtractor.Buy(round, Economy(round, perPlayer), Them.ToHashSet()));
	}

	[Fact]
	public void Should_return_no_buy_without_their_economy()
	{
		Assert.Null(OpponentFactsExtractor.Buy(2, null, Them.ToHashSet()));
	}

	#endregion
}
