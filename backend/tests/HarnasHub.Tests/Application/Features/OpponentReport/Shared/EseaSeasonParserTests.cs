using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class EseaSeasonParserTests
{
	#region Public Methods

	[Theory]
	[InlineData("S59 EU Open10 D - Regular Season", 59)]
	[InlineData("S58 EU Intermediate B - Playoffs", 58)]
	[InlineData("S57 EU Entry D - Regular Season", 57)]
	[InlineData("s60 na main a - regular season", 60)]
	[InlineData("ESEA Season 59: Open Division - Europe", 59)]
	[InlineData("ESEA S59 Advanced EU", 59)]
	[InlineData("ESEA League Season 50 Main", 50)]
	[InlineData("Season 52 ESEA Premier", 52)]
	public void Should_read_the_season_of_esea_league_competitions(string name, int season)
	{
		Assert.Equal(season, EseaSeasonParser.Parse(name));
	}

	[Theory]
	[InlineData("Eagle E-Sports | Classic Series | S01 T08")]
	[InlineData("IEM Beijing 2026 - Global Qualifier - EU Servers")]
	[InlineData("GGDAB WINGMAN - Pula 10500zl B")]
	[InlineData("5v5 RANKED")]
	[InlineData("ESEA League")]
	[InlineData("")]
	[InlineData(null)]
	public void Should_ignore_competitions_that_are_not_an_esea_season(string? name)
	{
		Assert.Null(EseaSeasonParser.Parse(name));
	}

	[Fact]
	public void Should_label_a_season_shortly()
	{
		Assert.Equal("S59", EseaSeasonParser.Label(59));
	}

	#endregion
}
