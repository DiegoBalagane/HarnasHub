#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class OpponentTendencyAggregatorTests
{
	#region Public Methods

	[Fact]
	public void Should_combine_every_demo_of_the_map()
	{
		var demo1 = Facts([TRound(2, MapArea.B, 80f), CtRound(14, [MapArea.A, MapArea.A, MapArea.B])]);
		var demo2 = Facts([TRound(2, MapArea.B, 80f)]);

		var map = OpponentTendencyAggregator.Aggregate(MapName.Mirage, [demo1, demo2]);

		Assert.Equal("Mirage", map.MapName);
		Assert.Equal(2, map.Demos);
		Assert.Equal(3, map.Rounds);
		Assert.Equal(2, map.T.Rounds);
		Assert.Equal(1, map.Ct.Rounds);
		Assert.True(map.HasZones);
		Assert.NotNull(map.TSpawnX);
	}

	[Fact]
	public void Should_sum_players_across_demos_and_assign_roles()
	{
		var awper = new OpponentPlayerFacts(11, "sniper", 20, 30, 2, 2, 12, 0, 0);
		var entry = new OpponentPlayerFacts(12, "entry", 20, 20, 6, 4, 0, 0, 0);
		var clutch = new OpponentPlayerFacts(13, "anchor", 20, 15, 0, 1, 1, 4, 2);
		var standIn = new OpponentPlayerFacts(19, "stand-in", 3, 2, 0, 0, 0, 0, 0);

		var players = OpponentTendencyAggregator.Players(
		[
			Facts([], [awper, entry, clutch, standIn]),
			Facts([], [awper with { Name = "sniper2" }])
		]);

		Assert.DoesNotContain(players, p => p.SteamId64 == "19");
		var sniper = players.Single(p => p.SteamId64 == "11");
		Assert.Equal(40, sniper.Rounds);
		Assert.Equal("sniper2", sniper.Name);
		Assert.Equal("AWP", sniper.Role);
		Assert.Equal(96, sniper.AwpKillShare);
		Assert.Equal("Entry", players.Single(p => p.SteamId64 == "12").Role);
		Assert.Equal(50, players.Single(p => p.SteamId64 == "12").EntryRate);
		Assert.Equal("Clutch", players.Single(p => p.SteamId64 == "13").Role);
	}

	[Fact]
	public void Should_leave_roles_empty_when_nobody_stands_out()
	{
		var players = OpponentTendencyAggregator.Players([Facts([], [new OpponentPlayerFacts(11, "a", 20, 10, 1, 1, 1, 1, 0)])]);

		Assert.Null(Assert.Single(players).Role);
		Assert.Equal(50, Assert.Single(players).OpeningWinRate);
	}

	#endregion
}
