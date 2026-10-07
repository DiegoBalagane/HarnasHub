using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OfficialMatchDetectorTests
{
	#region Private Fields

	private const string TeamId = "8bd4877a-4a03-4332-93a3-2a303d4073cc";
	private static readonly DateTime Now = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_find_the_side_whose_faction_is_the_faceit_team()
	{
		var asTeam1 = Official(TeamId, "S59 EU Open10 D - Regular Season", Them, Now);
		var asTeam2 = Match("de_nuke", Strangers(), ["r1", "r2", "r3", "r4", "r5"], 13, 9, Now);
		asTeam2.Team1FactionId = "someone-else";
		asTeam2.Team2FactionId = TeamId.ToUpperInvariant();

		Assert.Equal(1, OfficialMatchDetector.FindSide(asTeam1, TeamId));
		Assert.Equal(2, OfficialMatchDetector.FindSide(asTeam2, TeamId));
		Assert.Null(OfficialMatchDetector.FindSide(asTeam1, "other-team"));
		Assert.Null(OfficialMatchDetector.FindSide(asTeam1, null));
	}

	[Fact]
	public void Should_ignore_matchmaking_rooms_without_faction_ids_and_wingman_cups_of_the_team()
	{
		var matchmaking = Match("de_mirage", Them, Strangers(), 13, 5, Now);
		var wingman = Official(TeamId, "GGDAB WINGMAN - Pula 10500zl B", ["t1", "t2"], Now);

		Assert.Empty(OfficialMatchDetector.Detect([matchmaking, wingman], TeamId));
	}

	[Fact]
	public void Should_tag_official_games_with_their_esea_season_whoever_played_them()
	{
		var league = Official(TeamId, "S58 EU Open10 B - Regular Season", ["x1", "x2", "x3", "x4", "x5"], Now.AddDays(-40), won: false);
		var cup = Official(TeamId, "Eagle E-Sports | Classic Series | S01 T08", Them, Now.AddDays(-10));

		var games = OfficialMatchDetector.Detect([league, cup], TeamId);

		Assert.Equal(2, games.Count);
		Assert.All(games, g => Assert.True(g.Official));
		Assert.Null(games[0].Season);
		Assert.Equal(58, games[1].Season);
		Assert.False(games[1].Won);
		Assert.Equal(["x1", "x2", "x3", "x4", "x5"], games[1].SidePlayerIds);
		Assert.Equal(league.Id, games[1].RowId);
	}

	#endregion
}
