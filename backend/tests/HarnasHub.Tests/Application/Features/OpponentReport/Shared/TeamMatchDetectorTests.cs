using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class TeamMatchDetectorTests
{
	#region Private Fields

	private static readonly HashSet<string> Roster = [.. Them];
	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_find_the_side_with_at_least_three_roster_players()
	{
		Assert.Equal(1, TeamMatchDetector.FindSide(["t1", "t2", "t3", "x", "y"], ["a", "b", "c", "d", "e"], Roster));
		Assert.Equal(2, TeamMatchDetector.FindSide(["t1", "x", "y", "z", "w"], ["t2", "t3", "t4", "a", "b"], Roster));
	}

	[Fact]
	public void Should_not_count_two_roster_players_as_a_team_game()
	{
		Assert.Null(TeamMatchDetector.FindSide(["t1", "t2", "x", "y", "z"], ["a", "b", "c", "d", "e"], Roster));
	}

	[Fact]
	public void Should_ignore_internal_games_with_roster_players_on_both_sides()
	{
		var roster = new HashSet<string>(["t1", "t2", "t3", "t4", "t5", "t6"]);

		Assert.Null(TeamMatchDetector.FindSide(["t1", "t2", "t3", "x", "y"], ["t4", "t5", "t6", "a", "b"], roster));
	}

	[Fact]
	public void Should_flip_score_and_result_to_the_rosters_side_newest_first()
	{
		var older = Match("de_mirage", Strangers(), Them, 13, 7, Now.AddDays(-2));
		var newer = Match("de_nuke", Them, Strangers(), 13, 10, Now.AddDays(-1));

		var games = TeamMatchDetector.Detect([older, newer], Roster);

		Assert.Equal(2, games.Count);
		Assert.Equal((MapName?)MapName.Nuke, games[0].Map);
		Assert.True(games[0].Won);
		Assert.Equal((7, 13, false), (games[1].RoundsFor, games[1].RoundsAgainst, games[1].Won));
	}

	[Fact]
	public void Should_count_games_with_one_or_two_roster_players_as_solo()
	{
		var solo = Match("de_mirage", ["t1", "x", "y", "z", "w"], Strangers(), 13, 7, Now);
		var team = Match("de_mirage", Them, Strangers(), 13, 7, Now);
		var unrelated = Match("de_mirage", Strangers(), Strangers(), 13, 7, Now);

		Assert.Equal(1, TeamMatchDetector.CountSoloGames([solo, team, unrelated], Roster));
	}

	[Theory]
	[InlineData("de_mirage", MapName.Mirage)]
	[InlineData("de_dust2", MapName.Dust2)]
	[InlineData("Ancient", MapName.Ancient)]
	public void Should_parse_faceit_map_names(string raw, MapName expected)
	{
		Assert.Equal(expected, TeamMatchDetector.ParseMap(raw));
	}

	[Theory]
	[InlineData("de_train")]
	[InlineData("")]
	[InlineData(null)]
	public void Should_return_null_for_maps_outside_the_pool(string? raw)
	{
		Assert.Null(TeamMatchDetector.ParseMap(raw));
	}

	#endregion
}
