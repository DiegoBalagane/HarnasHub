#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class ClutchAnalyzerTests
{
	#region Public Methods

	[Fact]
	public void Should_record_a_lost_1v2_when_the_last_player_dies()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.CT)], [Kill(1, 3, 1, seconds: 10), Kill(1, 4, 2, seconds: 20)]);

		var result = ClutchAnalyzer.Analyze(context);

		var clutch = Assert.Single(result.Clutches, c => c.IsOurs);
		Assert.Equal("2", clutch.SteamId64);
		Assert.Equal(2, clutch.Versus);
		Assert.False(clutch.Won);
		Assert.Equal(1, result.OurAttempts);
		Assert.Equal(0, result.OurWon);
	}

	[Fact]
	public void Should_record_a_won_clutch_when_the_round_goes_to_that_side()
	{
		var context = AnalyzerTestSupport.Context(
			[Round(1, MapSide.T)],
			[Kill(1, 3, 1, seconds: 10), Kill(1, 2, 3, seconds: 20), Kill(1, 2, 4, seconds: 30)]);

		var result = ClutchAnalyzer.Analyze(context);

		var player = Assert.Single(result.Players);
		Assert.Equal(1, player.Won);
		Assert.Equal(2, player.BestWonVersus);
	}

	[Fact]
	public void Should_ignore_rounds_without_a_one_vs_x_situation()
	{
		// The factory's rosters are 2v2, so any death already leaves someone alone — only a round without deaths has no 1vX.
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], []);

		Assert.Empty(ClutchAnalyzer.Analyze(context).Clutches);
	}

	#endregion
}
