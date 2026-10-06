#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class MatchTimelineMapperTests
{
	#region Public Methods

	[Fact]
	public void Should_track_our_side_result_and_running_score_across_the_swap()
	{
		var timeline = MatchTimelineFactory.Timeline(
		[
			MatchTimelineFactory.Round(1, MapSide.T),
			MatchTimelineFactory.Round(13, MapSide.T, teamAOnT: false)
		]);

		var dto = MatchTimelineMapper.Map(MatchTimelineFactory.Stored(timeline), DemoTimelineFactory.TeamA);

		Assert.True(dto.OurTeamResolved);
		Assert.Equal(MapSide.T, dto.Rounds[0].OurSide);
		Assert.True(dto.Rounds[0].WeWon);
		Assert.Equal(MapSide.CT, dto.Rounds[1].OurSide);
		Assert.False(dto.Rounds[1].WeWon);
		Assert.Equal((1, 1), (dto.Rounds[1].OurScoreAfter, dto.Rounds[1].OpponentScoreAfter));
	}

	[Fact]
	public void Should_classify_each_teams_buy()
	{
		var timeline = MatchTimelineFactory.Timeline(
			[MatchTimelineFactory.Round(2, MapSide.CT)],
			[MatchTimelineFactory.Economy(2, 800, 4500)]);

		var round = MatchTimelineMapper.Map(MatchTimelineFactory.Stored(timeline), DemoTimelineFactory.TeamA).Rounds[0];

		Assert.Equal(BuyType.Eco, round.OurEconomy!.BuyType);
		Assert.Equal(MapSide.T, round.OurEconomy.Side);
		Assert.Equal(BuyType.Full, round.OpponentEconomy!.BuyType);
		Assert.Equal(9000, round.OpponentEconomy.EquipmentValue);
	}

	[Fact]
	public void Should_attribute_kills_to_us_by_side()
	{
		var timeline = MatchTimelineFactory.Timeline(
			[MatchTimelineFactory.Round(1, MapSide.T)],
			kills: [MatchTimelineFactory.Kill(1, 1, 3, isOpening: true), MatchTimelineFactory.Kill(1, 4, 2, seconds: 20f)]);

		var kills = MatchTimelineMapper.Map(MatchTimelineFactory.Stored(timeline), DemoTimelineFactory.TeamA).Rounds[0].Kills;

		Assert.True(kills[0].ByUs);
		Assert.True(kills[0].IsOpening);
		Assert.False(kills[1].ByUs);
	}

	[Fact]
	public void Should_fall_back_to_map_zones_when_the_game_did_not_report_the_site()
	{
		// Mirage B plant marker (225, 242) on the 1374x1196 radar image.
		var plant = new DemoBombEvent(30f, 1, "planter", 5, new DemoPosition(0, 0, 0, 225f / 1374, 242f / 1196));

		Assert.Equal(DemoBombSite.B, MatchTimelineMapper.ResolveSite(plant, MapName.Mirage));
		Assert.Equal(DemoBombSite.A, MatchTimelineMapper.ResolveSite(plant with { Site = DemoBombSite.A }, MapName.Mirage));
		Assert.Null(MatchTimelineMapper.ResolveSite(plant, null));
	}

	[Fact]
	public void Should_report_an_unresolved_team_and_fall_back_to_the_team_that_started_on_t()
	{
		var timeline = MatchTimelineFactory.Timeline([MatchTimelineFactory.Round(1, MapSide.CT)]);

		var dto = MatchTimelineMapper.Map(MatchTimelineFactory.Stored(timeline), []);

		Assert.False(dto.OurTeamResolved);
		Assert.Equal(MapSide.T, dto.Rounds[0].OurSide);
		Assert.False(dto.Rounds[0].WeWon);
	}

	#endregion
}
