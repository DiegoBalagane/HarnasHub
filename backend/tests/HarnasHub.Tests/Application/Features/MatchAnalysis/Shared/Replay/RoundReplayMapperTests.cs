#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Replay;

public class RoundReplayMapperTests
{
	#region Private Fields

	private static readonly ReplayPerspective TheirPerspective = new([], Them, "Team X");

	#endregion

	#region Public Methods

	[Fact]
	public void Should_return_null_for_a_round_the_timeline_does_not_have()
	{
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T)]);

		Assert.Null(RoundReplayMapper.Build(timeline, 2, TheirPerspective));
	}

	[Fact]
	public void Should_build_one_frame_per_second_with_every_sampled_player_indexed()
	{
		var timeline = Timeline(
			[Round(1, MapSide.T, MapSide.T)],
			positions: [Track(1, Them[0], MapSide.T, SiteA), Track(1, Others[0], MapSide.CT, SiteB)]);

		var replay = RoundReplayMapper.Build(timeline, 1, TheirPerspective)!;

		Assert.Equal(ReplayPositionsStatus.Available, replay.PositionsStatus);
		Assert.Equal(115, replay.DurationSeconds);
		Assert.Equal(116, replay.Frames.Count);
		Assert.All(replay.Frames.Select((f, i) => (f, i)), x => Assert.Equal(x.i, x.f.Second));
		Assert.Equal(2, replay.Frames[0].Players.Count);
		Assert.Empty(replay.Frames[31].Players);
		Assert.Equal(10, replay.Players.Count);

		var them = replay.Players[replay.Frames[0].Players.First(p => replay.Players[p.Player].Side == MapSide.T).Player];
		Assert.Equal(Them[0].ToString(), them.SteamId64);
		Assert.Equal(ReplayTeam.Opponent, them.Team);
		Assert.Equal(ReplayTeam.Unknown, replay.Players.Single(p => p.SteamId64 == Others[0].ToString()).Team);
		Assert.Equal(SiteA.X, replay.Frames[5].Players.Single(p => p.Player == them.Index).X);
		Assert.Equal("Team X", replay.OpponentName);
	}

	[Fact]
	public void Should_label_everyone_outside_our_team_as_the_opponent_and_report_our_side()
	{
		var timeline = Timeline([Round(1, MapSide.T, MapSide.CT)]);

		var replay = RoundReplayMapper.Build(timeline, 1, new ReplayPerspective(Others, [], "Team X"))!;

		Assert.Equal(MapSide.CT, replay.OurSide);
		Assert.All(replay.Players.Where(p => Others.Contains(long.Parse(p.SteamId64))), p => Assert.Equal(ReplayTeam.Ours, p.Team));
		Assert.All(replay.Players.Where(p => Them.Contains(long.Parse(p.SteamId64))), p => Assert.Equal(ReplayTeam.Opponent, p.Team));
	}

	[Fact]
	public void Should_drop_coordinates_on_a_map_without_a_verified_radar_fit_but_keep_kills()
	{
		var timeline = Timeline(
			[Round(1, MapSide.T, MapSide.T)],
			kills: [Kill(1, Them[0], Others[0], 12f, SiteA, SiteB)],
			positions: [Track(1, Them[0], MapSide.T, SiteA)]) with
		{ MapName = MapName.Dust2, RawMapName = "de_dust2" };

		var replay = RoundReplayMapper.Build(timeline, 1, TheirPerspective)!;

		Assert.Equal(ReplayPositionsStatus.UncalibratedMap, replay.PositionsStatus);
		Assert.Empty(replay.Frames);
		var kill = Assert.Single(replay.Kills);
		Assert.Null(kill.X);
		Assert.Equal(Them[0].ToString(), replay.Players[kill.Killer!.Value].SteamId64);
	}

	[Fact]
	public void Should_report_positions_as_not_recorded_for_a_timeline_parsed_without_them()
	{
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T)]);

		var replay = RoundReplayMapper.Build(timeline, 1, TheirPerspective)!;

		Assert.Equal(ReplayPositionsStatus.NotRecorded, replay.PositionsStatus);
		Assert.Empty(replay.Frames);
	}

	[Fact]
	public void Should_map_grenades_with_recorded_detonation_and_skip_decoys()
	{
		var smoke = Grenade(1, 1, Them[1], SiteA, seconds: 20f) with { DetonationSecond = 22f };
		var decoy = Grenade(2, 1, Them[1], SiteB, type: DemoGrenadeType.Decoy);
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T)], grenades: [smoke, decoy], positions: [Track(1, Them[0], MapSide.T, SiteA)]);

		var replay = RoundReplayMapper.Build(timeline, 1, TheirPerspective)!;

		var grenade = Assert.Single(replay.Grenades);
		Assert.Equal(22f, grenade.DetonateSecond);
		Assert.Equal(40f, grenade.EndSecond);
		Assert.False(grenade.TimesApproximate);
		Assert.Equal(SiteA.X, grenade.LandX);
		Assert.Equal(Them[1].ToString(), replay.Players[grenade.Thrower!.Value].SteamId64);
	}

	[Fact]
	public void Should_mark_the_explosion_at_the_end_of_the_round()
	{
		var round = Round(1, MapSide.T, MapSide.T, Plant(DemoBombSite.A, 60f)) with { EndReason = DemoRoundEndReason.BombExploded };
		var timeline = Timeline([round]);

		var replay = RoundReplayMapper.Build(timeline, 1, TheirPerspective)!;

		Assert.NotNull(replay.Bomb);
		Assert.Equal(60f, replay.Bomb!.PlantSecond);
		Assert.Equal(DemoBombSite.A, replay.Bomb.Site);
		Assert.Equal(115f, replay.Bomb.ExplodeSecond);
		Assert.Null(replay.Bomb.DefuseSecond);
	}

	#endregion
}
