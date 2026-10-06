using System.Text.Json;
using HarnasHub.Infrastructure.Faceit;
using Xunit;

namespace HarnasHub.Tests.Infrastructure.Faceit;

public class FaceitJsonTests
{
	#region Public Methods

	[Fact]
	public void Should_read_a_player_with_cs2_details()
	{
		var player = FaceitJson.ReadPlayer(Parse("""
			{ "player_id": "p1", "nickname": "Alpha", "steam_id_64": "111",
			  "games": { "cs2": { "faceit_elo": 2150, "skill_level": 10, "game_player_id": "76561198000000001" } } }
			"""));

		Assert.NotNull(player);
		Assert.Equal(("p1", "Alpha", "76561198000000001", (int?)2150, (int?)10), (player.PlayerId, player.Nickname, player.SteamId64, player.Elo, player.SkillLevel));
	}

	[Fact]
	public void Should_return_null_for_a_player_without_id()
	{
		Assert.Null(FaceitJson.ReadPlayer(Parse("""{ "nickname": "Ghost" }""")));
	}

	[Fact]
	public void Should_read_history_with_unix_finish_times()
	{
		var items = FaceitJson.ReadHistory(Parse("""
			{ "items": [ { "match_id": "1-abc", "finished_at": 1759320000, "competition_type": "championship",
			  "competition_name": "ESEA", "status": "FINISHED" }, { "status": "FINISHED" } ] }
			"""));

		var item = Assert.Single(items);
		Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1759320000).UtcDateTime, item.FinishedAtUtc);
		Assert.Equal("championship", item.CompetitionType);
	}

	[Fact]
	public void Should_read_string_typed_match_stats_per_map()
	{
		var maps = FaceitJson.ReadMatchStats(Parse("""
			{ "rounds": [ {
			  "match_round": "1",
			  "round_stats": { "Map": "de_mirage", "Score": "13 / 7", "Winner": "f1" },
			  "teams": [
			    { "team_id": "f1", "team_stats": { "Team": "team_Alpha", "Final Score": "13" },
			      "players": [ { "player_id": "p1", "nickname": "Alpha",
			        "player_stats": { "Kills": "25", "Deaths": "12", "Assists": "4", "ADR": "97.4", "Headshots %": "55",
			          "Triple Kills": "2", "Quadro Kills": "1", "Penta Kills": "0", "MVPs": "5" } } ] },
			    { "team_id": "f2", "team_stats": { "Team": "team_Beta" }, "players": [] } ] } ] }
			"""));

		var map = Assert.Single(maps);
		Assert.Equal(("de_mirage", 1), (map.MapName, map.MapNumber));
		Assert.Equal((13, true), (map.Teams[0].Score, map.Teams[0].Won));
		Assert.Equal((7, false), (map.Teams[1].Score, map.Teams[1].Won));
		var line = Assert.Single(map.Teams[0].Players);
		Assert.Equal((25, 12, 4, 2, 1, 5), (line.Kills, line.Deaths, line.Assists, line.TripleKills, line.QuadroKills, line.Mvps));
		Assert.Equal(97.4, line.Adr);
		Assert.Equal(55, line.HeadshotPercent);
	}

	[Fact]
	public void Should_read_both_factions_of_a_match_room()
	{
		var match = FaceitJson.ReadMatch(Parse("""
			{ "match_id": "1-abc", "teams": {
			  "faction1": { "faction_id": "f1", "name": "team_Alpha", "roster": [ { "player_id": "p1", "nickname": "Alpha", "game_player_id": "111" } ] },
			  "faction2": { "faction_id": "f2", "name": "team_Beta", "roster": [] } } }
			"""));

		Assert.NotNull(match);
		Assert.Equal(["team_Alpha", "team_Beta"], match.Factions.Select(f => f.Name));
		Assert.Equal("111", match.Factions[0].Players[0].SteamId64);
	}

	#endregion

	#region Private Methods

	private static JsonElement Parse(string json) => JsonDocument.Parse(json).RootElement;

	#endregion
}
