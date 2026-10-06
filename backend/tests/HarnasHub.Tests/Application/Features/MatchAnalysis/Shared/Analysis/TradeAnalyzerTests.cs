#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class TradeAnalyzerTests
{
	#region Public Methods

	[Fact]
	public void Should_credit_a_trade_when_a_teammate_kills_the_killer_within_the_window()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], [Kill(1, 3, 1, seconds: 10), Kill(1, 2, 3, seconds: 14)]);

		var result = TradeAnalyzer.Analyze(context);

		Assert.Equal(1, result.OurDeaths);
		Assert.Equal(1, result.OurTradedDeaths);
		Assert.Equal(1, result.OurTradeKills);
		Assert.Equal("2", result.Players.First().SteamId64);
	}

	[Fact]
	public void Should_not_trade_outside_the_window()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], [Kill(1, 3, 1, seconds: 10), Kill(1, 2, 3, seconds: 16)]);

		var result = TradeAnalyzer.Analyze(context);

		Assert.Equal(0, result.OurTradedDeaths);
		Assert.Equal(0, result.OurTradeKills);
	}

	[Fact]
	public void Should_track_opponent_deaths_separately()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], [Kill(1, 1, 3, seconds: 10), Kill(1, 4, 1, seconds: 12)]);

		var result = TradeAnalyzer.Analyze(context);

		Assert.Equal(1, result.OpponentDeaths);
		Assert.Equal(1, result.OpponentTradedDeaths);
		Assert.Equal(1, result.OurDeaths);
		Assert.Equal(0, result.OurTradedDeaths);
	}

	#endregion
}
