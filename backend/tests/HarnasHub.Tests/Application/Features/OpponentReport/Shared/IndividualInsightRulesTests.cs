using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class IndividualInsightRulesTests
{
	#region Public Methods

	[Fact]
	public void SoloAvoidance_should_call_a_ban_on_a_map_most_players_avoid_and_LikelyBans_should_not_repeat_it()
	{
		var input = Input([Row("Ancient", 0, "Ban"), Row("Nuke", 1, "Ban")], theirComfort: [Comfort("Ancient", 5, 0, 4)]);

		var insight = Assert.Single(IndividualInsightRules.SoloAvoidance(input));
		var likely = Assert.Single(OpponentInsightRules.LikelyBans(input));

		Assert.Equal("4 z 5 graczy unika Ancient także w meczach solo → prawie pewny ban", insight.Text);
		Assert.Contains("pewność wysoka", insight.Evidence);
		Assert.Equal("Nie grają Nuke — prawie pewny ban", likely.Text);
	}

	[Fact]
	public void SoloAvoidance_should_stay_silent_when_they_play_the_map_as_a_team()
	{
		Assert.Empty(IndividualInsightRules.SoloAvoidance(Input([Row("Ancient", 6)], theirComfort: [Comfort("Ancient", 5, 0, 4)])));
	}

	[Fact]
	public void SoloComfortPick_should_flag_a_map_most_players_play_and_win_solo()
	{
		var input = Input([Row("Mirage", 1)], theirComfort: [Comfort("Mirage", 5, 4, 0, winRate: 58)]);

		var insight = Assert.Single(IndividualInsightRules.SoloComfortPick(input));

		Assert.Equal("4 z 5 graczy gra Mirage regularnie solo (śr. 58% wygranych) → możliwy pick", insight.Text);
	}

	[Fact]
	public void PlayerForm_should_name_the_hottest_and_the_coldest_player()
	{
		var input = Input([], theirPlayers:
		[
			Player("f0xelon", new PlayerRecentFormDto(10, 15, 1.45, 1.05, 60, 48, 0.4, 12, "Up")),
			Player("mild", new PlayerRecentFormDto(10, 15, 1.2, 1.0, 50, 50, 0.2, 0, "Up")),
			Player("cold", new PlayerRecentFormDto(10, 15, 0.8, 1.1, 30, 50, -0.3, -20, "Down"))
		]);

		var insights = IndividualInsightRules.PlayerForm(input).ToList();

		Assert.Equal(2, insights.Count);
		Assert.Equal("f0xelon w formie: K/D 1.45 w ostatnich 10 meczach (wcześniej 1.05)", insights[0].Text);
		Assert.Equal("Warning", insights[0].Severity);
		Assert.Equal("cold bez formy: K/D 0.80 w ostatnich 10 meczach (wcześniej 1.10)", insights[1].Text);
	}

	[Fact]
	public void OurStrongMap_should_pick_our_best_solo_kd_outside_permanent_bans()
	{
		var input = Input(
			[Row("Inferno", 3), Row("Nuke", 3) with { PoolStatus = "Ban" }],
			ourPlayers: [Player("Kacper", null, Map("Inferno", 8, 1.3), Map("Nuke", 9, 1.8), Map("Mirage", 3, 2.0))]);

		var insight = Assert.Single(IndividualInsightRules.OurStrongMap(input));

		Assert.Equal("Nasz Kacper na Inferno: K/D 1.30 solo — mocna mapa", insight.Text);
	}

	[Fact]
	public void Build_should_include_individual_rules_and_still_cap_the_tldr()
	{
		var input = Input(
			[Row("Ancient", 0, "Ban")],
			theirComfort: [Comfort("Ancient", 5, 0, 4)],
			theirPlayers: [Player("f0xelon", new PlayerRecentFormDto(10, 15, 1.45, 1.05, 60, 48, 0.4, 12, "Up"))]);

		var insights = OpponentInsightRules.Build(input);

		Assert.True(insights.Count <= OpponentInsightRules.MaxInsights);
		Assert.Equal("PlayerForm", insights[0].Kind);
		Assert.Contains(insights, i => i.Kind == "SoloAvoidance");
		Assert.DoesNotContain(insights, i => i.Kind == "LikelyBan");
	}

	#endregion

	#region Private Methods

	private static InsightInput Input(
		List<MapComparisonDto> maps,
		List<MapComfortDto>? theirComfort = null,
		List<PlayerFormDto>? theirPlayers = null,
		List<PlayerFormDto>? ourPlayers = null) =>
		new(20, maps, [], new OpponentFormDto([], null, []),
			new IndividualFormDto(new TeamIndividualFormDto(theirPlayers ?? [], theirComfort ?? []), new TeamIndividualFormDto(ourPlayers ?? [], [])));

	private static MapComparisonDto Row(string map, int theirGames, string prediction = "Neutral") =>
		new(map, theirGames, 0, null, null, 0, null, null, 0, 0, null, 0, 0, null, 0, "Low", prediction, "", 0, "Neutral", []);

	private static MapComfortDto Comfort(string map, int rated, int regulars, int avoiding, double? winRate = null) =>
		new(map, rated, regulars, avoiding, 10, winRate, 1.1, Enumerable.Range(0, regulars).Select(i => $"r{i}").ToList(), Enumerable.Range(0, avoiding).Select(i => $"a{i}").ToList());

	private static PlayerFormDto Player(string nickname, PlayerRecentFormDto? form, params PlayerMapFormDto[] maps) =>
		new(nickname, nickname, 2000, 10, 40, 10, 30, 50, 1.1, 80, 45, DateTime.UtcNow, form, maps.ToList());

	private static PlayerMapFormDto Map(string map, int soloGames, double soloKd) =>
		new(map, soloGames, 20, soloGames / 2, 50, soloKd, 85, 50, DateTime.UtcNow, 0, soloGames, null, 55, null, soloKd);

	#endregion
}
