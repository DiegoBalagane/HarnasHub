using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OpponentTeamGamesTests
{
	#region Private Fields

	private const string TeamId = "team-1";
	private const string S59 = "S59 EU Open10 D - Regular Season";
	private const string S58 = "S58 EU Open10 B - Regular Season";
	private static readonly DateTime Now = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

	/// <summary>The current season's five: four linked members plus "k1", who isn't on the FACEIT team page.</summary>
	private static readonly string[] Season = ["b1", "b2", "b3", "b4", "k1"];

	/// <summary>The FACEIT team page: the season's members, last season's player "old" and PUG friends "r1"–"r3".</summary>
	private static readonly HashSet<string> Roster = ["b1", "b2", "b3", "b4", "old", "r1", "r2", "r3"];

	#endregion

	#region Public Methods

	[Fact]
	public void Should_take_the_lineup_from_the_current_esea_season_with_appearances()
	{
		var matches = new List<FaceitMatch>
		{
			Official(TeamId, S59, Season, Now.AddDays(-2), "m59a"),
			Official(TeamId, S59, ["b1", "b2", "b3", "b4", "old"], Now.AddDays(-5), "m59b"),
			// A BO3 of the same match is still one appearance.
			Official(TeamId, S59, Season, Now.AddDays(-5), "m59b", "de_nuke"),
			Official(TeamId, S58, ["b1", "b2", "old", "x1", "x2"], Now.AddDays(-60), "m58")
		};

		var lineup = OpponentTeamGames.Select(matches, TeamId, Roster, Now).Lineup;

		Assert.Equal(["b1", "b2", "b3", "b4", "k1", "old"], lineup.ActiveIds.Order());
		Assert.Equal(SeasonLineupResolver.EseaSeasonSource, lineup.Dto.Source);
		Assert.Equal("S59", lineup.Dto.Season);
		Assert.Equal(S59, lineup.Dto.SeasonCompetition);
		Assert.Equal(2, lineup.Dto.WindowGames);
		Assert.Equal(3, lineup.Dto.OfficialMatches);
		Assert.Contains(lineup.Dto.Active, p => p.PlayerId == "b1" && p.RecentTeamGames == 2 && p.TeamGames == 3);
		Assert.Contains(lineup.Dto.Active, p => p.PlayerId == "k1" && p.RecentTeamGames == 2);
		Assert.Contains(lineup.Dto.Active, p => p.PlayerId == "old" && p.RecentTeamGames == 1);
		Assert.Equal(["r1", "r2", "r3"], lineup.Dto.Inactive.Select(p => p.PlayerId).Order());
		Assert.StartsWith("Skład z sezonu ESEA S59", lineup.Dto.Basis);
	}

	[Fact]
	public void Should_count_official_games_of_every_season_plus_games_of_three_lineup_players_together()
	{
		var current = Official(TeamId, S59, Season, Now.AddDays(-2));
		var lastSeason = Official(TeamId, S58, ["old", "x1", "x2", "x3", "x4"], Now.AddDays(-60));
		var together = Match("de_inferno", ["b1", "b2", "k1", "s1", "s2"], Strangers(), 13, 4, Now.AddDays(-1));
		// Two of the lineup with three team-page "randoms" isn't the team.
		var randoms = Match("de_inferno", ["b1", "b2", "r1", "r2", "r3"], Strangers(), 13, 4, Now.AddDays(-3));
		var solo = Match("de_inferno", ["b3", .. Strangers()[..4]], Strangers(), 13, 4, Now.AddDays(-3));

		var games = OpponentTeamGames.Select([current, lastSeason, together, randoms, solo], TeamId, Roster, Now).Games;

		Assert.Equal([together.Id, current.Id, lastSeason.Id], games.Select(g => g.RowId));
		Assert.Equal([false, true, true], games.Select(g => g.Official));
		Assert.Equal(1, games[0].Side);
	}

	[Fact]
	public void Should_fall_back_to_the_active_lineup_of_linked_players_without_official_games()
	{
		var roster = Them.ToHashSet();
		var matches = Enumerable.Range(0, 4).Select(i => Match("de_mirage", Them, Strangers(), 13, 5, Now.AddDays(-i))).ToList();

		var selection = OpponentTeamGames.Select(matches, TeamId, roster, Now);

		Assert.Equal(OpponentTeamGames.TeamGamesSource, selection.Lineup.Dto.Source);
		Assert.Null(selection.Lineup.Dto.OfficialMatches);
		Assert.Equal(Them, selection.Lineup.ActiveIds.Order());
		Assert.Equal(4, selection.Games.Count);
		Assert.All(selection.Games, g => Assert.False(g.Official));
	}

	[Fact]
	public void Should_take_the_lineup_from_recent_official_games_when_the_team_plays_no_esea()
	{
		var cups = Enumerable.Range(0, 3)
			.Select(i => Official(TeamId, "Eagle E-Sports | Classic Series | S01 T08", ["b1", "b2", "b3", "b4", "r1"], Now.AddDays(-i)))
			.ToList();

		var lineup = OpponentTeamGames.Select(cups, TeamId, Roster, Now).Lineup;

		Assert.Equal(OpponentTeamGames.OfficialMatchesSource, lineup.Dto.Source);
		Assert.Equal(3, lineup.Dto.OfficialMatches);
		Assert.Equal(["b1", "b2", "b3", "b4", "r1"], lineup.ActiveIds.Order());
		Assert.Contains("meczów oficjalnych drużyny", lineup.Dto.Basis);
	}

	#endregion
}
