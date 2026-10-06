#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.Shared.Matching;

public class RoundSignatureExtractorTests
{
	#region Public Methods

	[Fact]
	public void Should_extract_nothing_on_a_map_without_a_verified_radar_fit()
	{
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T)]) with { MapName = MapName.Inferno };

		Assert.Empty(RoundSignatureExtractor.Extract(timeline, Them));
	}

	[Fact]
	public void Should_extract_nothing_without_our_team()
	{
		Assert.Empty(RoundSignatureExtractor.Extract(Timeline([Round(1, MapSide.T, MapSide.T)]), []));
	}

	[Fact]
	public void Should_keep_only_our_positions_and_grenades_within_their_windows()
	{
		var timeline = Timeline(
			[Round(1, MapSide.T, MapSide.CT)],
			grenades:
			[
				Grenade(1, 1, Them[0], SiteA, seconds: 30f),
				Grenade(2, 1, Them[0], SiteB, seconds: 45f),
				Grenade(3, 1, Others[0], Mid, seconds: 10f),
				Grenade(4, 1, Them[1], Mid, seconds: 10f, type: DemoGrenadeType.Decoy)
			],
			positions: [Track(1, Them[0], MapSide.T, SiteA), Track(1, Others[0], MapSide.CT, SiteB)]);

		var signature = Assert.Single(RoundSignatureExtractor.Extract(timeline, Them));

		Assert.Equal(MapSide.T, signature.OurSide);
		Assert.False(signature.WeWon);
		Assert.True(signature.HasPositions);
		Assert.Equal(RoundSignatureExtractor.PositionWindowSeconds + 1, signature.PlayerPoints.Count);
		Assert.All(signature.PlayerPoints, p => Assert.Equal(SiteA.X, p.X));
		var grenade = Assert.Single(signature.Grenades);
		Assert.Equal(GrenadeType.Smoke, grenade.Type);
		Assert.Equal(SiteA.X, grenade.X);
	}

	[Fact]
	public void Should_flag_timelines_without_position_samples()
	{
		var signature = Assert.Single(RoundSignatureExtractor.Extract(Timeline([Round(1, MapSide.CT, MapSide.CT)]), Them));

		Assert.False(signature.HasPositions);
		Assert.Equal(MapSide.CT, signature.OurSide);
		Assert.True(signature.WeWon);
	}

	#endregion
}
