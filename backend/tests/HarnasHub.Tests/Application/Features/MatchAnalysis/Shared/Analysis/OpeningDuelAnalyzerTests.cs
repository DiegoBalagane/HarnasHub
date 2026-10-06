#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class OpeningDuelAnalyzerTests
{
	#region Public Methods

	[Fact]
	public void Should_count_won_and_lost_openings_per_side()
	{
		var context = AnalyzerTestSupport.Context(
			[Round(1, MapSide.T), Round(2, MapSide.T, teamAOnT: false)],
			[Kill(1, 1, 3, isOpening: true), Kill(2, 3, 1, isOpening: true, teamAOnT: false)]);

		var result = OpeningDuelAnalyzer.Analyze(context);

		Assert.Equal(2, result.Duels.Count);
		Assert.True(result.Duels[0].WonByUs);
		Assert.Equal(MapSide.T, result.Duels[0].OurSide);
		Assert.False(result.Duels[1].WonByUs);
		Assert.Equal(MapSide.CT, result.Duels[1].OurSide);
		Assert.Equal(2, result.Buckets.Count);
		var player = result.Players.Single(p => p.SteamId64 == "1");
		Assert.Equal(1, player.Won);
		Assert.Equal(1, player.Lost);
	}

	[Fact]
	public void Should_skip_openings_between_two_opponents()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], [Kill(1, 3, 4, isOpening: true)]);

		Assert.Empty(OpeningDuelAnalyzer.Analyze(context).Duels);
	}

	#endregion
}
