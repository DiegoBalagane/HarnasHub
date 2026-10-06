#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Demos;

public class DemoPlayerTrackTests
{
	#region Public Methods

	[Fact]
	public void Should_decode_the_sample_of_a_covered_second()
	{
		var track = new DemoPlayerTrack(3, 9, MapSide.CT, 10, [1, 2, 3, 4, 5, 6], [0.1f, 0.2f, 0.3f, 0.4f], [100, 80], ["awp", ""]);

		var sample = track.At(11);

		Assert.NotNull(sample);
		Assert.Equal((4, 5, 6), (sample.WorldX, sample.WorldY, sample.WorldZ));
		Assert.Equal(0.3f, sample.RadarX);
		Assert.Equal(0.4f, sample.RadarY);
		Assert.Equal(80, sample.Health);
		Assert.Null(sample.Weapon);
		Assert.Equal("awp", track.At(10)!.Weapon);
	}

	[Theory]
	[InlineData(9, false)]
	[InlineData(10, true)]
	[InlineData(11, true)]
	[InlineData(12, false)]
	public void Should_cover_only_the_sampled_seconds(int second, bool expected)
	{
		var track = new DemoPlayerTrack(3, 9, MapSide.CT, 10, [1, 2, 3, 4, 5, 6], null, [100, 80], ["", ""]);

		Assert.Equal(expected, track.Covers(second));
		Assert.Equal(expected, track.At(second) is not null);
	}

	[Fact]
	public void Should_return_no_radar_position_for_an_uncalibrated_track()
	{
		var track = new DemoPlayerTrack(1, 9, MapSide.T, 0, [1, 2, 3], null, [100], ["knife"]);

		Assert.Null(track.At(0)!.RadarX);
	}

	#endregion
}
