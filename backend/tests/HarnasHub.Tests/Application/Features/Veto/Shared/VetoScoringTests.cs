using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.Shared;

public class VetoScoringTests
{
	#region Public Methods

	[Fact]
	public void Should_rank_a_strong_core_map_first_and_a_pool_ban_last()
	{
		var result = VetoScoring.Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Core, wins: 8, losses: 2),
			Input(MapName.Dust2, MapPoolStatus.Ban),
			Input(MapName.Nuke, MapPoolStatus.Playable, wins: 3, losses: 3)
		]);

		Assert.Equal(["Mirage", "Nuke", "Dust2"], result.Select(m => m.MapName));
		Assert.Equal("Pick", result[0].Recommendation);
		Assert.Equal("Ban", result[^1].Recommendation);
		Assert.Equal(-100, result[^1].Score);
		Assert.Contains("Stały ban w puli map", result[^1].Reasons);
	}

	[Fact]
	public void Should_flag_exactly_three_bans_counting_pool_bans_first()
	{
		var maps = Enum.GetValues<MapName>()
			.Select((map, index) => Input(map, map == MapName.Cache ? MapPoolStatus.Ban : MapPoolStatus.Playable, wins: index, losses: 6 - index))
			.ToList();

		var result = VetoScoring.Suggest(maps);

		var bans = result.Where(m => m.Recommendation == "Ban").Select(m => m.MapName).ToList();
		Assert.Equal(VetoScoring.OurBanCount, bans.Count);
		Assert.Contains("Cache", bans);
		Assert.Equal(VetoScoring.PickCount, result.Count(m => m.Recommendation == "Pick"));
	}

	[Fact]
	public void Should_never_pick_a_map_with_a_non_positive_score()
	{
		var result = VetoScoring.Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Learning, wins: 0, losses: 4),
			Input(MapName.Nuke, null)
		]);

		Assert.DoesNotContain(result, m => m.Recommendation == "Pick");
	}

	[Fact]
	public void Should_penalize_the_opponents_picks_and_reward_their_bans()
	{
		var neutral = VetoScoring.Suggest([Input(MapName.Inferno, MapPoolStatus.Playable, wins: 2, losses: 2)]).Single();
		var theirPick = VetoScoring.Suggest([Input(MapName.Inferno, MapPoolStatus.Playable, wins: 2, losses: 2, opponentPicks: 2)]).Single();
		var theirBan = VetoScoring.Suggest([Input(MapName.Inferno, MapPoolStatus.Playable, wins: 2, losses: 2, opponentBans: 2)]).Single();

		Assert.Equal(neutral.Score - 12, theirPick.Score);
		Assert.Equal(neutral.Score + 8, theirBan.Score);
		Assert.Contains(theirPick.Reasons, r => r.StartsWith("Przeciwnik wybierał ją 2×"));
	}

	[Fact]
	public void Should_smooth_a_single_game_instead_of_treating_it_as_100_percent()
	{
		var oneWin = VetoScoring.Suggest([Input(MapName.Ancient, null, wins: 1)]).Single();
		var tenWins = VetoScoring.Suggest([Input(MapName.Ancient, null, wins: 10)]).Single();

		Assert.True(oneWin.Score < tenWins.Score);
		Assert.Contains("Bilans ogólny 1-0 (100% wygranych)", oneWin.Reasons);
	}

	[Fact]
	public void Should_use_the_head_to_head_record_and_tactic_count()
	{
		var result = VetoScoring.Suggest([Input(MapName.Anubis, null, headToHeadWins: 2, tacticCount: 3)]).Single();

		Assert.Contains("Z tym przeciwnikiem 2-0", result.Reasons);
		Assert.Contains("3 taktyki w bibliotece", result.Reasons);
		// -5 no games, +16 head-to-head, +6 tactics.
		Assert.Equal(17, result.Score);
	}

	#endregion

	#region Private Methods

	private static MapVetoInput Input(
		MapName map,
		MapPoolStatus? status,
		int wins = 0,
		int losses = 0,
		int headToHeadWins = 0,
		int opponentPicks = 0,
		int opponentBans = 0,
		int tacticCount = 0) =>
		new(map, status, wins, losses, 0, headToHeadWins, 0, opponentPicks, opponentBans, tacticCount);

	#endregion
}
