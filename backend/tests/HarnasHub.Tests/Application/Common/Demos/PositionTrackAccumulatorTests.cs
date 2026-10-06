#region Usings

using HarnasHub.Application.Common.Demos;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Demos;

public class PositionTrackAccumulatorTests
{
	#region Public Methods

	[Fact]
	public void Should_build_one_contiguous_track_per_player_and_round()
	{
		var accumulator = new PositionTrackAccumulator();
		for (var second = 0; second < 3; second++)
		{
			accumulator.Add(1, second, 7, MapSide.T, 100.4f + second, -50.6f, 10f, 100 - second, "ak47");
			accumulator.EndSecond(second);
		}

		var track = Assert.Single(accumulator.Build((x, y) => (0.123456f, 0.5f)));

		Assert.Equal(1, track.RoundNumber);
		Assert.Equal(7, track.SteamId64);
		Assert.Equal(MapSide.T, track.Side);
		Assert.Equal(0, track.StartSecond);
		Assert.Equal(new[] { 100, -51, 10, 101, -51, 10, 102, -51, 10 }, track.World);
		Assert.Equal(0.1235f, track.Radar![0]);
		Assert.Equal(new[] { 100, 99, 98 }, track.Health);
		Assert.Equal(2, track.EndSecond());
	}

	[Fact]
	public void Should_close_a_track_when_the_player_is_missing_and_open_a_new_one_when_seen_again()
	{
		var accumulator = new PositionTrackAccumulator();
		accumulator.Add(1, 0, 7, MapSide.CT, 0, 0, 0, 100, null);
		accumulator.EndSecond(0);
		accumulator.EndSecond(1);
		accumulator.Add(1, 2, 7, MapSide.CT, 0, 0, 0, 100, null);
		accumulator.EndSecond(2);

		var tracks = accumulator.Build((_, _) => null);

		Assert.Equal(2, tracks.Count);
		Assert.Equal(new[] { 0, 2 }, tracks.Select(t => t.StartSecond));
		Assert.All(tracks, t => Assert.Null(t.Radar));
		Assert.Equal(string.Empty, tracks[0].Weapons[0]);
	}

	[Fact]
	public void Should_split_tracks_by_round_and_drop_everything_on_reset()
	{
		var accumulator = new PositionTrackAccumulator();
		accumulator.Add(1, 5, 7, MapSide.T, 0, 0, 0, 100, null);
		accumulator.EndRound();
		accumulator.Add(2, 5, 7, MapSide.CT, 0, 0, 0, 100, null);

		Assert.Equal(2, accumulator.Build((_, _) => null).Count);

		accumulator.Reset();
		Assert.Empty(accumulator.Build((_, _) => null));
	}

	[Fact]
	public void Should_clamp_health_into_zero_to_hundred()
	{
		var accumulator = new PositionTrackAccumulator();
		accumulator.Add(1, 0, 7, MapSide.T, 0, 0, 0, 250, null);

		Assert.Equal(100, Assert.Single(accumulator.Build((_, _) => null)).Health[0]);
	}

	#endregion
}
