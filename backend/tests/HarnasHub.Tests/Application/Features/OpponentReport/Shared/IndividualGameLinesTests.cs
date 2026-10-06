using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class IndividualGameLinesTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_split_team_and_solo_games_and_read_the_result_from_the_players_side()
	{
		var team = Match("de_mirage", Them, Strangers(), 13, 7, Now.AddDays(-2));
		var solo = Match("de_ancient", Strangers(), ["t1", "x2", "x3", "x4", "x5"], 13, 9, Now.AddDays(-1));
		var outside = Match("de_nuke", ["t1"], Strangers(), 13, 0, Now.AddDays(-1));
		var stats = new[] { Stat(team, "t1", 20, 10, 90), Stat(solo, "t1", 15, 18, 70), Stat(outside, "t1", 30, 5, 120), Stat(team, "zz", 1, 1, 1) };

		var lines = IndividualGameLines.Build([team, solo], stats, Them.ToHashSet());

		Assert.Equal(2, lines.Count);
		Assert.Equal((MapName.Mirage, true, true), (lines[0].Map!.Value, lines[0].TeamGame, lines[0].Won));
		Assert.Equal((MapName.Ancient, false, false), (lines[1].Map!.Value, lines[1].TeamGame, lines[1].Won));
	}

	#endregion
}
