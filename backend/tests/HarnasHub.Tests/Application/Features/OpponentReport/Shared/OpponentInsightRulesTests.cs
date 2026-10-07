using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OpponentInsightRulesTests
{
	#region Private Fields

	private static readonly OpponentFormDto NoForm = new([], null, []);

	#endregion

	#region Public Methods

	[Fact]
	public void LowSample_should_fire_below_three_team_games()
	{
		Assert.Single(OpponentInsightRules.LowSample(Input(2, [])));
		Assert.Empty(OpponentInsightRules.LowSample(Input(3, [])));
	}

	[Fact]
	public void MainMaps_should_name_the_two_most_played_maps_covering_half_their_games()
	{
		var insight = Assert.Single(OpponentInsightRules.MainMaps(Input(20, [Row("Mirage", 8, share: 40), Row("Ancient", 6, share: 30), Row("Nuke", 2, share: 10)])));

		Assert.Equal("Grają głównie Mirage i Ancient (70% meczów)", insight.Text);
		Assert.Equal("Info", insight.Severity);
	}

	[Fact]
	public void MainMaps_should_stay_silent_when_their_games_are_spread_out()
	{
		Assert.Empty(OpponentInsightRules.MainMaps(Input(20, [Row("Mirage", 4, share: 20), Row("Ancient", 4, share: 20)])));
	}

	[Fact]
	public void LikelyBans_should_combine_predicted_bans()
	{
		var insight = Assert.Single(OpponentInsightRules.LikelyBans(Input(20, [Row("Nuke", 0, prediction: "Ban"), Row("Anubis", 1, prediction: "Ban")])));

		Assert.Equal("Nie grają Nuke i Anubis — prawie pewne bany", insight.Text);
	}

	[Fact]
	public void LikelyBans_should_need_the_larger_team_sample()
	{
		Assert.Empty(OpponentInsightRules.LikelyBans(Input(SampleThresholds.MinTeamGamesForAvoidance - 3, [Row("Ancient", 1, prediction: "Ban")])));
	}

	[Fact]
	public void LikelyBans_should_skip_a_map_the_lineup_plays_a_lot_lifetime()
	{
		var ancient = Row("Ancient", 1, prediction: "Ban") with { TheirLifetime = new MapLifetimeDto(5, 113, 52, 1.12, 22.6, true) };

		Assert.Empty(OpponentInsightRules.LikelyBans(Input(20, [ancient])));
	}

	[Fact]
	public void Dangers_should_need_enough_of_their_games_and_flag_our_thin_sample()
	{
		var thin = Row("Mirage", 4, theirWinRate: 75, ourWinRate: 0, advantage: -30, confidence: "Medium");
		var solid = Row("Nuke", 9, theirWinRate: 78, ourWinRate: 0, advantage: -25, confidence: "Medium") with { OurGames = 2, OurWins = 0, Recommendation = "Ban" };

		var insight = Assert.Single(OpponentInsightRules.Dangers(Input(20, [thin, solid])));

		Assert.Equal("Uwaga na Nuke: oni 78%, my za mało danych (2 mecze) — kandydat do bana", insight.Text);
	}

	[Fact]
	public void Dangers_should_not_call_a_map_a_ban_candidate_when_our_veto_keeps_it()
	{
		var kept = Row("Anubis", 9, theirWinRate: 78, ourWinRate: 0, advantage: -25, confidence: "Medium") with { OurGames = 4, OurWins = 3 };

		var insight = Assert.Single(OpponentInsightRules.Dangers(Input(20, [kept])));

		Assert.EndsWith("ich mocna mapa, ale nie ma jej w naszych banach — przygotujcie na nią plan", insight.Text);
	}

	[Fact]
	public void Opportunities_should_flag_maps_where_they_are_weak_and_we_are_strong()
	{
		var insight = Assert.Single(OpponentInsightRules.Opportunities(Input(10, [Row("Inferno", 8, theirWinRate: 37.5, ourWinRate: 70, advantage: 20)])));

		Assert.Equal("Słabi na Inferno (38%), my 70% → pick", insight.Text);
		Assert.Equal("High", insight.Severity);
	}

	[Fact]
	public void Opportunities_should_ignore_tiny_samples()
	{
		Assert.Empty(OpponentInsightRules.Opportunities(Input(10, [Row("Inferno", 2, theirWinRate: 0, ourWinRate: 100, advantage: 30)])));
	}

	[Fact]
	public void Dangers_should_flag_a_clear_edge_for_them_unless_confidence_is_low()
	{
		var danger = Row("Mirage", 10, theirWinRate: 70, ourWinRate: 40, advantage: -18, confidence: "Medium");

		Assert.Single(OpponentInsightRules.Dangers(Input(10, [danger])));
		Assert.Empty(OpponentInsightRules.Dangers(Input(10, [danger with { Confidence = "Low" }])));
	}

	[Fact]
	public void ThreatPlayer_should_pick_the_highest_adr_with_enough_games()
	{
		var players = new List<MapPlayersToWatchDto>
		{
			new("Mirage", [new("p1", "Alpha", 5, 1.3, 92, 50, 0.4), new("p2", "Beta", 2, 2.0, 120, 60, 1)])
		};

		var insight = Assert.Single(OpponentInsightRules.ThreatPlayer(Input(10, [], players)));

		Assert.Equal("Groźny: Alpha (ADR 92 na Mirage)", insight.Text);
	}

	[Fact]
	public void Form_should_report_a_hot_streak()
	{
		var games = Enumerable.Range(0, 5).Select(i => new FormGameDto($"m{i}", DateTime.UtcNow, "Mirage", 13, 5, i != 4, null)).ToList();

		var insight = Assert.Single(OpponentInsightRules.Form(Input(10, [], form: new OpponentFormDto(games, "W4", []))));

		Assert.Equal("W formie: 4 z 5 ostatnich meczów wygranych", insight.Text);
	}

	[Fact]
	public void RosterChanges_should_list_new_players()
	{
		var insight = Assert.Single(OpponentInsightRules.RosterChanges(Input(10, [], form: new OpponentFormDto([], null, ["Newbie"]))));

		Assert.Contains("Newbie", insight.Text);
	}

	[Fact]
	public void Build_should_cap_at_five_with_the_most_severe_first()
	{
		List<MapComparisonDto> maps =
		[
			Row("Inferno", 8, theirWinRate: 30, ourWinRate: 70, advantage: 25, share: 40),
			Row("Overpass", 8, theirWinRate: 35, ourWinRate: 65, advantage: 20, share: 40),
			Row("Mirage", 10, theirWinRate: 70, ourWinRate: 40, advantage: -18, confidence: "High"),
			Row("Nuke", 0, prediction: "Ban")
		];

		var insights = OpponentInsightRules.Build(Input(2, maps, form: new OpponentFormDto([], null, ["Newbie"])));

		Assert.Equal(OpponentInsightRules.MaxInsights, insights.Count);
		Assert.Equal("High", insights[0].Severity);
		Assert.Equal("High", insights[1].Severity);
		Assert.Equal("Warning", insights[2].Severity);
	}

	#endregion

	#region Private Methods

	private static InsightInput Input(
		int teamGames,
		List<MapComparisonDto> maps,
		List<MapPlayersToWatchDto>? players = null,
		OpponentFormDto? form = null) =>
		new(teamGames, maps, players ?? [], form ?? NoForm);

	private static MapComparisonDto Row(
		string map,
		int theirGames,
		double? theirWinRate = 50,
		double? ourWinRate = 50,
		double advantage = 0,
		double share = 0,
		string confidence = "Medium",
		string prediction = "Neutral") =>
		new(map, theirGames, (int)Math.Round(theirGames * (theirWinRate ?? 0) / 100), theirWinRate, null, share, null, null,
			10, 10 * (ourWinRate ?? 0) / 100, ourWinRate, 10, 0, null, advantage, confidence, prediction, "", 0, "Neutral", []);

	#endregion
}
