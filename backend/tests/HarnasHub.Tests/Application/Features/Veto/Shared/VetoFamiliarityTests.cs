#region Usings

using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Veto.Shared;

public class VetoFamiliarityTests
{
	#region Public Methods

	[Fact]
	public void Should_ban_maps_we_dont_play_before_a_known_map_with_a_poor_record()
	{
		var result = Suggest(ProductionPool());

		Assert.Equal(["Cache", "Dust2", "Inferno"], Bans(result));
		var dust2 = result.Single(m => m.MapName == "Dust2");
		Assert.Equal("nie gramy tej mapy (1 mecz) — ban w pierwszej kolejności", dust2.Note);
		Assert.Contains("Nie gramy tej mapy (1 mecz drużynowo, brak statusu w puli) — ban w pierwszej kolejności", dust2.Reasons);
		var ancient = result.Single(m => m.MapName == "Ancient");
		Assert.Equal("Neutral", ancient.Recommendation);
		Assert.Equal("znamy mapę (6 meczów), mimo słabego bilansu 17% — nie banujemy, dopóki są mapy, których nie gramy", ancient.Note);
	}

	[Fact]
	public void Should_not_ban_our_comfortable_map_for_the_opponents_strength()
	{
		var anubis = Suggest(ProductionPool()).Single(m => m.MapName == "Anubis");

		Assert.NotEqual("Ban", anubis.Recommendation);
		Assert.StartsWith("ich mocna mapa, ale i nasza (3-1, 4 mecze) — nie banujemy", anubis.Note);
		Assert.Contains("Rywal jest tu mocny, ale to też nasza mapa (3-1, 4 mecze) — nie banujemy jej", anubis.Reasons);
	}

	[Fact]
	public void Should_ban_a_known_map_only_after_the_unplayed_ones_and_say_so()
	{
		var result = Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Core, wins: 6, losses: 2),
			Input(MapName.Dust2, null, losses: 1),
			Input(MapName.Ancient, null, wins: 1, losses: 5),
			Input(MapName.Nuke, MapPoolStatus.Playable, wins: 3, losses: 3),
			Input(MapName.Inferno, MapPoolStatus.Playable, wins: 4, losses: 2)
		]);

		Assert.Equal(["Ancient", "Dust2"], Bans(result));
		Assert.Equal("znamy mapę (6 meczów), ale bilans 17% — ban dopiero po mapach, których nie gramy", result.Single(m => m.MapName == "Ancient").Note);
	}

	[Fact]
	public void Should_let_pool_ban_and_learning_outrank_familiarity()
	{
		var result = Suggest(
		[
			Input(MapName.Mirage, MapPoolStatus.Ban, wins: 10),
			Input(MapName.Ancient, MapPoolStatus.Learning, wins: 4, losses: 4),
			Input(MapName.Dust2, null),
			Input(MapName.Inferno, null),
			Input(MapName.Nuke, MapPoolStatus.Core, wins: 5, losses: 3),
			Input(MapName.Anubis, MapPoolStatus.Playable, wins: 3, losses: 3),
			Input(MapName.Cache, MapPoolStatus.Playable, wins: 2, losses: 2)
		]);

		// Pool ban first, then the Learning map (-25) and one unplayed map fill the remaining two slots.
		Assert.Equal(["Ancient", "Dust2", "Mirage"], Bans(result));
		Assert.Equal("Ban", result.Single(m => m.MapName == "Mirage").Recommendation);
	}

	[Fact]
	public void Should_count_regular_solo_play_as_familiarity_without_a_coach_pool()
	{
		var result = Suggest(
		[
			Input(MapName.Mirage, null, wins: 6, losses: 2),
			Input(MapName.Dust2, null) with { OurPlaysIndividually = true },
			Input(MapName.Inferno, null),
			Input(MapName.Nuke, null, wins: 2, losses: 1)
		]);

		Assert.Equal(["Inferno"], Bans(result));
		Assert.Contains("Znamy tę mapę (większość składu gra ją regularnie solo)", result.Single(m => m.MapName == "Dust2").Reasons);
	}

	[Fact]
	public void Should_trust_the_coach_pool_over_solo_play()
	{
		var result = Suggest(
		[
			Input(MapName.Mirage, null, wins: 6, losses: 2),
			Input(MapName.Ancient, MapPoolStatus.Playable, wins: 2, losses: 2) with { TheirGames = 10, TheirWins = 8, TheirWinRate = 0.8 },
			Input(MapName.Cache, null) with { OurPlaysIndividually = true },
			Input(MapName.Inferno, null) with { OurPlaysIndividually = true },
			Input(MapName.Anubis, null) with { OurPlaysIndividually = true },
			Input(MapName.Dust2, null),
			Input(MapName.Nuke, null)
		]);

		Assert.DoesNotContain("Ancient", Bans(result));
		Assert.DoesNotContain("Cache", result.Where(m => m.Recommendation == "Pick").Select(m => m.MapName));
		Assert.All(Bans(result), map => Assert.Contains(map, new[] { "Cache", "Inferno", "Anubis", "Dust2", "Nuke" }));
	}

	[Fact]
	public void Should_ignore_familiarity_without_any_familiar_map()
	{
		var result = Suggest([Input(MapName.Dust2, null), Input(MapName.Inferno, null), Input(MapName.Nuke, null)]);

		Assert.Empty(Bans(result));
		Assert.All(result, m => Assert.DoesNotContain("nie gramy", m.Note));
	}

	[Fact]
	public void Should_need_most_of_the_lineup_playing_solo_for_individual_familiarity()
	{
		var three = Comfort(regulars: 3);
		var two = Comfort(regulars: 2);
		var experienced = new MapLifetime(MapName.Dust2, 5, 400, 210, 1.05, 0.2);

		Assert.True(VetoFamiliarity.PlaysIndividually(three, null));
		Assert.False(VetoFamiliarity.PlaysIndividually(two, null));
		Assert.True(VetoFamiliarity.PlaysIndividually(two, experienced));
		Assert.False(VetoFamiliarity.PlaysIndividually(null, experienced));
	}

	#endregion

	#region Private Methods

	/// <summary>The reported production case: Anubis 3-1, Ancient 1-5 known; Dust2 (1), Inferno (0), Cache (2) not played.</summary>
	private static List<MapVetoInput> ProductionPool() =>
	[
		Input(MapName.Mirage, MapPoolStatus.Core, wins: 6, losses: 2),
		Input(MapName.Nuke, MapPoolStatus.Playable, wins: 3, losses: 3),
		Input(MapName.Ancient, null, wins: 1, losses: 5),
		Input(MapName.Anubis, null, wins: 3, losses: 1) with { TheirGames = 10, TheirWins = 8, TheirWinRate = 0.75 },
		Input(MapName.Dust2, null, losses: 1),
		Input(MapName.Inferno, null),
		Input(MapName.Cache, null, wins: 1, losses: 1)
	];

	private static List<MapVetoSuggestionDto> Suggest(List<MapVetoInput> maps) => VetoScoring.Suggest(maps);

	private static List<string> Bans(List<MapVetoSuggestionDto> result) =>
		result.Where(m => m.Recommendation == "Ban").Select(m => m.MapName).Order().ToList();

	private static MapComfort Comfort(int regulars) =>
		new(MapName.Dust2, 5, regulars, 0, 0.15, 0.5, 0.5, 1.0, [], []);

	private static MapVetoInput Input(MapName map, MapPoolStatus? status, int wins = 0, int losses = 0) =>
		new(map, status, wins, losses, 0, 0, 0, 0, 0, 0);

	#endregion
}
