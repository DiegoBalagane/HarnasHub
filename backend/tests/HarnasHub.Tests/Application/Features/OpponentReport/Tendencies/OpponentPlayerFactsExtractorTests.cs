#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class OpponentPlayerFactsExtractorTests
{
	#region Public Methods

	[Fact]
	public void Should_count_rounds_kills_openings_and_awp_kills_per_player()
	{
		var timeline = Timeline(
			[Round(1, MapSide.T, MapSide.T), Round(2, MapSide.T, MapSide.CT)],
			kills:
			[
				Kill(1, 11, 21, 10f, weapon: "awp", isOpening: true),
				Kill(1, 11, 22, 20f, weapon: "awp"),
				Kill(2, 23, 12, 10f, isOpening: true)
			]);

		var players = OpponentPlayerFactsExtractor.Extract(timeline, Them.ToHashSet()).ToDictionary(p => p.SteamId64);

		Assert.Equal(5, players.Count);
		Assert.Equal(2, players[11].Rounds);
		Assert.Equal(2, players[11].Kills);
		Assert.Equal(1, players[11].OpeningKills);
		Assert.Equal(2, players[11].AwpKills);
		Assert.Equal(1, players[12].OpeningDeaths);
		Assert.Equal("p11", players[11].Name);
	}

	[Fact]
	public void Should_find_the_last_player_alive_against_remaining_enemies()
	{
		var kills = new[]
		{
			Kill(1, 21, 11, 10f),
			Kill(1, 21, 12, 11f),
			Kill(1, 21, 13, 12f),
			Kill(1, 21, 14, 13f)
		};

		Assert.Equal(15, OpponentPlayerFactsExtractor.ClutchPlayer(Them.ToHashSet(), Others.ToHashSet(), kills));
	}

	[Fact]
	public void Should_not_count_a_clutch_once_every_enemy_is_dead()
	{
		var kills = Others.Select((id, i) => Kill(1, 11, id, 10f + i)).ToArray();

		Assert.Null(OpponentPlayerFactsExtractor.ClutchPlayer(Them.ToHashSet(), Others.ToHashSet(), kills));
	}

	[Fact]
	public void Should_credit_a_won_clutch()
	{
		var kills = new[] { Kill(1, 21, 11, 10f), Kill(1, 21, 12, 11f), Kill(1, 21, 13, 12f), Kill(1, 21, 14, 13f), Kill(1, 15, 21, 20f) };
		var timeline = Timeline([Round(1, MapSide.T, MapSide.T)], kills: kills);

		var clutcher = OpponentPlayerFactsExtractor.Extract(timeline, Them.ToHashSet()).Single(p => p.SteamId64 == 15);

		Assert.Equal(1, clutcher.ClutchAttempts);
		Assert.Equal(1, clutcher.ClutchWins);
	}

	#endregion
}
