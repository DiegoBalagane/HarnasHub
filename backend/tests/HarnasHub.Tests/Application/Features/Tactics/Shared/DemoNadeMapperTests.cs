#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Enums;
using static HarnasHub.Tests.Application.Features.Tactics.DemoTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.Shared;

/// <summary>Covers the demo-to-wizard mapping: per-side score tracking across a side swap and grenade filtering.</summary>
public class DemoNadeMapperTests
{
	#region Public Methods

	[Fact]
	public void ScoresAfterEachRound_follows_a_team_across_the_side_swap()
	{
		// Team A: T in rounds 1-2 (wins round 1), CT in round 3 (wins it).
		var rounds = new[]
		{
			Round(1, MapSide.T),
			Round(2, MapSide.CT),
			Round(3, MapSide.CT, teamAOnT: false)
		};

		var scores = DemoNadeMapper.ScoresAfterEachRound(rounds);

		Assert.Equal((1, 0), scores[0]);
		Assert.Equal((1, 1), scores[1]);
		// Team B (1 win) is now on T, team A (2 wins) on CT.
		Assert.Equal((1, 2), scores[2]);
	}

	[Fact]
	public void ScoresAfterEachRound_ignores_rounds_without_a_winner()
	{
		var scores = DemoNadeMapper.ScoresAfterEachRound([Round(1, null), Round(2, MapSide.T)]);

		Assert.Equal((0, 0), scores[0]);
		Assert.Equal((1, 0), scores[1]);
	}

	[Fact]
	public void Map_groups_grenades_by_round_ordered_by_time()
	{
		var timeline = Timeline(
			[Round(1, MapSide.T), Round(2, MapSide.CT)],
			[Grenade(1, 1, seconds: 20f), Grenade(2, 1, DemoGrenadeType.Flash, seconds: 5f), Grenade(3, 2, DemoGrenadeType.Incendiary)]);

		var result = DemoNadeMapper.Map(MapName.Mirage, timeline);

		Assert.Equal(MapName.Mirage, result.MapName);
		Assert.Equal([2, 1], result.Rounds[0].Grenades.Select(g => g.Id));
		Assert.Equal(GrenadeType.Molotov, result.Rounds[1].Grenades.Single().Type);
		Assert.Equal("1", result.Rounds[0].Grenades[0].ThrowerSteamId);
		Assert.Equal(0.5f, result.Rounds[0].Grenades[0].LandX);
	}

	[Fact]
	public void Map_drops_decoys_unknown_sides_and_grenades_without_a_landing()
	{
		var timeline = Timeline(
			[Round(1, MapSide.T)],
			[
				Grenade(1, 1, DemoGrenadeType.Decoy),
				Grenade(2, 1, hasLanding: false),
				Grenade(3, 1, side: null),
				Grenade(4, 1, DemoGrenadeType.HighExplosive)
			]);

		var result = DemoNadeMapper.Map(MapName.Mirage, timeline);

		var grenade = Assert.Single(result.Rounds[0].Grenades);
		Assert.Equal(4, grenade.Id);
		Assert.Equal(GrenadeType.Frag, grenade.Type);
	}

	[Fact]
	public void Map_keeps_rounds_without_grenades()
	{
		var result = DemoNadeMapper.Map(MapName.Mirage, Timeline([Round(1, MapSide.CT)], []));

		Assert.Empty(Assert.Single(result.Rounds).Grenades);
	}

	#endregion
}
