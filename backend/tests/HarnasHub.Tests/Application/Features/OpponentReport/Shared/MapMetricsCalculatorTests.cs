using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class MapMetricsCalculatorTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_return_every_pool_map_with_zeros_when_there_are_no_games()
	{
		var metrics = MapMetricsCalculator.Calculate([]);

		Assert.Equal(Enum.GetValues<MapName>().Length, metrics.Count);
		Assert.All(metrics.Values, m =>
		{
			Assert.Equal(0, m.Games);
			Assert.Null(m.WinRate);
			Assert.Equal(0.5, m.SmoothedWinRate);
			Assert.Equal(0, m.Share);
		});
	}

	[Fact]
	public void Should_compute_rate_round_diff_last_played_and_share()
	{
		List<TeamGame> games =
		[
			Game(MapName.Mirage, 13, 5, Now.AddDays(-1)),
			Game(MapName.Mirage, 10, 13, Now.AddDays(-3)),
			Game(MapName.Nuke, 13, 11, Now.AddDays(-2)),
			Game(null, 13, 0, Now.AddDays(-4))
		];

		var mirage = MapMetricsCalculator.Calculate(games)[MapName.Mirage];

		Assert.Equal(2, mirage.Games);
		Assert.Equal(1, mirage.Wins);
		Assert.Equal(0.5, mirage.WinRate);
		Assert.Equal(2.5, mirage.AvgRoundDiff);
		Assert.Equal(Now.AddDays(-1), mirage.LastPlayedAtUtc);
		Assert.Equal(0.5, mirage.Share);
	}

	[Fact]
	public void Should_compare_the_last_five_games_with_the_earlier_ones()
	{
		var games = Enumerable.Range(0, 8)
			.Select(i => Game(MapName.Inferno, i < 5 ? 13 : 5, i < 5 ? 5 : 13, Now.AddDays(-i)))
			.ToList();

		var inferno = MapMetricsCalculator.Calculate(games)[MapName.Inferno];

		Assert.Equal(1.0, inferno.RecentWinRate);
		Assert.Equal(0.0, inferno.EarlierWinRate);
		Assert.Equal(1.0, inferno.Trend);
	}

	[Fact]
	public void Should_have_no_trend_without_earlier_games()
	{
		var metrics = MapMetricsCalculator.Calculate([Game(MapName.Anubis, 13, 2, Now)]);

		Assert.Null(metrics[MapName.Anubis].Trend);
	}

	#endregion

	#region Private Methods

	private static TeamGame Game(MapName? map, int roundsFor, int roundsAgainst, DateTime at) =>
		new(Guid.NewGuid().ToString(), at, map, map?.ToString(), roundsFor, roundsAgainst, roundsFor > roundsAgainst, [], null);

	#endregion
}
