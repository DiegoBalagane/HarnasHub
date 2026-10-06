using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class PlayersToWatchCalculatorTests
{
	#region Public Methods

	[Fact]
	public void Should_rank_players_by_adr_and_skip_single_game_samples()
	{
		List<PlayerGameLine> lines =
		[
			Line("a", MapName.Mirage, 20, 10, 80),
			Line("a", MapName.Mirage, 22, 12, 90),
			Line("b", MapName.Mirage, 15, 15, 70),
			Line("b", MapName.Mirage, 15, 15, 72),
			Line("c", MapName.Mirage, 40, 5, 150)
		];

		var mirage = Assert.Single(PlayersToWatchCalculator.Calculate(lines));

		Assert.Equal("Mirage", mirage.MapName);
		Assert.Equal(["a", "b"], mirage.Players.Select(p => p.PlayerId));
		var top = mirage.Players[0];
		Assert.Equal((2, 85.0, 1.91), (top.Games, top.Adr!.Value, top.KdRatio));
	}

	[Fact]
	public void Should_fall_back_to_kd_without_adr_and_cap_at_three()
	{
		var lines = new[] { "a", "b", "c", "d" }
			.SelectMany((id, i) => Enumerable.Repeat(Line(id, MapName.Nuke, 10 + i * 5, 10, null), 2))
			.ToList();

		var nuke = Assert.Single(PlayersToWatchCalculator.Calculate(lines));

		Assert.Equal(PlayersToWatchCalculator.TopPerMap, nuke.Players.Count);
		Assert.Equal("d", nuke.Players[0].PlayerId);
		Assert.Null(nuke.Players[0].Adr);
	}

	#endregion

	#region Private Methods

	private static PlayerGameLine Line(string id, MapName map, int kills, int deaths, double? adr) =>
		new(id, $"nick-{id}", map, kills, deaths, adr, 50, 1);

	#endregion
}
