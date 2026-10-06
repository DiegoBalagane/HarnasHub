#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;
using HarnasHub.Infrastructure.Demos.Collectors;

#endregion

namespace HarnasHub.Tests.Infrastructure.Demos.Collectors;

/// <summary>Covers the parser-independent parts of the collectors: landing resolution of tracked grenades, fire-to-molotov
/// matching, team/end-reason mapping and the timeline builder's radar conversion.</summary>
public class GrenadeCollectorLogicTests
{
	#region Public Methods

	[Fact]
	public void Detonation_position_wins_over_last_seen_position_and_is_never_overwritten()
	{
		var grenade = new TrackedGrenade(1, 1, DemoGrenadeType.Smoke, 3f, (0, 0, 0)) { LastWorld = (5, 5, 0) };

		grenade.Detonate(10, 20, 30);
		grenade.Detonate(99, 99, 99);

		var result = grenade.ToPublic(new DemoTimelineBuilder(null, null));
		Assert.Equal(10, result.Landing!.WorldX);
		Assert.Null(result.Landing.RadarX);
		Assert.Equal("?", result.ThrowerName);
	}

	[Fact]
	public void Detonation_time_is_kept_from_the_first_detonation_and_absent_without_one()
	{
		var detonated = new TrackedGrenade(1, 1, DemoGrenadeType.Smoke, 3f, (0, 0, 0));
		detonated.Detonate(10, 20, 30, 5.5f);
		detonated.Detonate(99, 99, 99, 9f);
		var rolling = new TrackedGrenade(2, 1, DemoGrenadeType.Flash, 3f, (0, 0, 0)) { LastWorld = (5, 5, 0) };

		var builder = new DemoTimelineBuilder(null, null);
		Assert.Equal(5.5f, detonated.ToPublic(builder).DetonationSecond);
		Assert.Null(rolling.ToPublic(builder).DetonationSecond);
	}

	[Fact]
	public void Landing_falls_back_to_last_seen_position()
	{
		var grenade = new TrackedGrenade(1, 1, DemoGrenadeType.Flash, 3f, (0, 0, 0)) { LastWorld = (5, 6, 7) };

		var result = grenade.ToPublic(new DemoTimelineBuilder(null, null));

		Assert.Equal(6, result.Landing!.WorldY);
	}

	[Fact]
	public void FindFireSource_picks_the_nearest_unburnt_fire_grenade_of_the_round()
	{
		var near = new TrackedGrenade(1, 2, DemoGrenadeType.Molotov, 0f, (0, 0, 0)) { LastWorld = (100, 100, 0) };
		var far = new TrackedGrenade(2, 2, DemoGrenadeType.Incendiary, 0f, (0, 0, 0)) { LastWorld = (300, 100, 0) };
		var otherRound = new TrackedGrenade(3, 1, DemoGrenadeType.Molotov, 0f, (0, 0, 0)) { LastWorld = (100, 100, 0) };
		var smoke = new TrackedGrenade(4, 2, DemoGrenadeType.Smoke, 0f, (0, 0, 0)) { LastWorld = (100, 100, 0) };
		var burnt = new TrackedGrenade(5, 2, DemoGrenadeType.Molotov, 0f, (0, 0, 0));
		burnt.Detonate(100, 100, 0);

		var match = TrackedGrenade.FindFireSource([far, otherRound, smoke, burnt, near], 2, 110, 100, 400);

		Assert.Same(near, match);
	}

	[Fact]
	public void FindFireSource_returns_null_beyond_the_max_distance()
	{
		var grenade = new TrackedGrenade(1, 1, DemoGrenadeType.Molotov, 0f, (0, 0, 0)) { LastWorld = (1000, 0, 0) };

		Assert.Null(TrackedGrenade.FindFireSource([grenade], 1, 0, 0, 400));
	}

	[Theory]
	[InlineData(1, DemoRoundEndReason.BombExploded)]
	[InlineData(7, DemoRoundEndReason.BombDefused)]
	[InlineData(8, DemoRoundEndReason.Elimination)]
	[InlineData(9, DemoRoundEndReason.Elimination)]
	[InlineData(12, DemoRoundEndReason.TimeExpired)]
	[InlineData(17, DemoRoundEndReason.Surrender)]
	[InlineData(10, DemoRoundEndReason.Other)]
	public void EndReason_collapses_game_reasons(int raw, DemoRoundEndReason expected)
	{
		Assert.Equal(expected, DemoTeams.EndReason(raw));
	}

	[Fact]
	public void WinnerSide_maps_team_numbers()
	{
		Assert.Equal(MapSide.T, DemoTeams.WinnerSide(2));
		Assert.Equal(MapSide.CT, DemoTeams.WinnerSide(3));
		Assert.Null(DemoTeams.WinnerSide(1));
	}

	[Fact]
	public void Builder_converts_to_radar_fractions_on_calibrated_maps_only()
	{
		var mirage = new DemoTimelineBuilder("de_mirage", MapName.Mirage);
		var unknown = new DemoTimelineBuilder("de_vertigo", null);

		Assert.NotNull(mirage.ToPosition(-500, 0, 0).RadarX);
		Assert.Null(unknown.ToPosition(-500, 0, 0).RadarX);
		Assert.True(mirage.Build().IsRadarCalibrated);
		Assert.False(unknown.Build().IsRadarCalibrated);
	}

	#endregion
}
