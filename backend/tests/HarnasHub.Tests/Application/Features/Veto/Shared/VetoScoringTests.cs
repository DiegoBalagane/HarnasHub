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
	public void Should_ignore_a_record_below_the_minimum_sample_and_say_so()
	{
		var oneWin = VetoScoring.Suggest([Input(MapName.Ancient, null, wins: 1)]).Single();
		var tenWins = VetoScoring.Suggest([Input(MapName.Ancient, null, wins: 10)]).Single();

		Assert.Equal(0, oneWin.Score);
		Assert.Contains("Nasz bilans 1-0 — za mało danych (1 mecz), nie wpływa na rekomendację", oneWin.Reasons);
		Assert.Equal("za mało danych (my: 1 mecz)", oneWin.Note);
		Assert.Contains("Nasz bilans 10-0 (100% wygranych w 10 meczach)", tenWins.Reasons);
		Assert.True(tenWins.Score > 0);
	}

	[Fact]
	public void Should_not_ban_a_map_for_one_or_two_lost_games()
	{
		var result = VetoScoring.Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Core, wins: 6, losses: 2),
			Input(MapName.Nuke, MapPoolStatus.Playable, losses: 2),
			Input(MapName.Inferno, MapPoolStatus.Playable, wins: 3, losses: 3),
			Input(MapName.Ancient, MapPoolStatus.Playable, wins: 1, losses: 1)
		]);

		var nuke = result.Single(m => m.MapName == "Nuke");
		Assert.NotEqual("Ban", nuke.Recommendation);
		Assert.Contains("Nasz bilans 0-2 — za mało danych (2 mecze), nie wpływa na rekomendację", nuke.Reasons);
		Assert.Equal("w puli: gramy, za mało danych (my: 2 mecze)", nuke.Note);
	}

	[Fact]
	public void Should_ban_on_a_meaningful_losing_record_or_a_learning_map()
	{
		var result = VetoScoring.Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Core, wins: 6, losses: 2),
			Input(MapName.Nuke, MapPoolStatus.Playable, wins: 1, losses: 7),
			Input(MapName.Ancient, MapPoolStatus.Learning),
			Input(MapName.Inferno, MapPoolStatus.Playable, wins: 3, losses: 3),
			Input(MapName.Anubis, MapPoolStatus.Playable)
		]);

		Assert.Equal(["Ancient", "Nuke"], result.Where(m => m.Recommendation == "Ban").Select(m => m.MapName).Order());
		Assert.Equal("w puli: w przygotowaniu, my: brak meczów", result.Single(m => m.MapName == "Ancient").Note);
	}

	[Fact]
	public void Should_let_the_pool_status_outweigh_a_mediocre_record()
	{
		var result = VetoScoring.Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Core, wins: 2, losses: 4),
			Input(MapName.Inferno, MapPoolStatus.Playable, wins: 5, losses: 1)
		]);

		Assert.Equal("Mirage", result[0].MapName);
		Assert.Contains("Mapa komfortowa w puli (pewniak)", result[0].Reasons);
		Assert.StartsWith("mapa komfortowa w puli", result[0].Note);
	}

	[Fact]
	public void Should_never_ban_on_solo_form_alone()
	{
		var result = VetoScoring.Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Core, wins: 6, losses: 2),
			Input(MapName.Nuke, null) with { OurSoloPrior = 0.4, OurPlaysIndividually = true },
			Input(MapName.Inferno, MapPoolStatus.Playable, wins: 3, losses: 3)
		]);

		var nuke = result.Single(m => m.MapName == "Nuke");
		Assert.Equal(-5, nuke.Score);
		Assert.NotEqual("Ban", nuke.Recommendation);
		Assert.Contains(nuke.Reasons, r => r.StartsWith("Forma indywidualna naszych graczy"));
	}

	[Fact]
	public void Should_use_the_head_to_head_record_and_tactic_count()
	{
		var result = VetoScoring.Suggest([Input(MapName.Anubis, null, headToHeadWins: 2, tacticCount: 3)]).Single();

		Assert.Contains("Z tym przeciwnikiem 2-0", result.Reasons);
		Assert.Contains("3 taktyki w bibliotece", result.Reasons);
		Assert.Contains("Brak naszych meczów na tej mapie", result.Reasons);
		// +16 head-to-head, +6 tactics; no games and no pool status cost nothing.
		Assert.Equal(22, result.Score);
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
