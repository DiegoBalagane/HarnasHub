using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class MapAdvantageTests
{
	#region Public Methods

	[Fact]
	public void Should_pull_a_small_perfect_sample_towards_fifty_percent()
	{
		Assert.Equal(4.5 / 7, MapAdvantage.SmoothedWinRate(2, 2), 6);
	}

	[Fact]
	public void Should_be_exactly_fifty_percent_without_games()
	{
		Assert.Equal(0.5, MapAdvantage.SmoothedWinRate(0, 0));
	}

	[Fact]
	public void Should_subtract_their_smoothed_rate_from_ours()
	{
		var advantage = MapAdvantage.Advantage(ourWins: 7, ourGames: 10, theirWins: 3, theirGames: 10);

		Assert.Equal((9.5 / 15) - (5.5 / 15), advantage, 6);
	}

	[Theory]
	[InlineData(0, ConfidenceLevel.Low)]
	[InlineData(2, ConfidenceLevel.Low)]
	[InlineData(3, ConfidenceLevel.Medium)]
	[InlineData(9, ConfidenceLevel.Medium)]
	[InlineData(10, ConfidenceLevel.High)]
	public void Should_grade_confidence_by_the_smaller_sample(int minGames, ConfidenceLevel expected)
	{
		Assert.Equal(expected, MapAdvantage.Confidence(minGames));
	}

	#endregion
}
