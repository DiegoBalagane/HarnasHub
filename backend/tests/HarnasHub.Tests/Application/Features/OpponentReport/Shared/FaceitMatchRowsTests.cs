using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class FaceitMatchRowsTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_match_history_factions_to_scoreboard_teams_by_players_not_by_order()
	{
		var factions = new List<FaceitHistoryFaction>
		{
			new("rival-team", ["x1", "x2", "x3", "x4", "x5"]),
			new("our-team", ["t1", "t2", "t3", "t4", "t5"])
		};

		Assert.Equal("our-team", FaceitMatchRows.FactionId(["t1", "t2", "t3", "t4", "sub"], factions));
		Assert.Equal("rival-team", FaceitMatchRows.FactionId(["x1"], factions));
		Assert.Null(FaceitMatchRows.FactionId(["nobody"], factions));
	}

	[Fact]
	public void Should_backfill_faction_and_competition_ids_of_rows_cached_before_they_were_stored()
	{
		var row = Match("de_mirage", Them, ["x1", "x2", "x3", "x4", "x5"], 13, 7, Now, "m1");
		var other = Match("de_nuke", Them, Strangers(), 13, 7, Now, "m2");
		var item = new FaceitHistoryItem("m1", Now, "championship", "S59 EU Open10 D - Regular Season", "FINISHED")
		{
			CompetitionId = "champ-59",
			Factions = [new("rival-team", ["x1", "x2", "x3", "x4", "x5"]), new("their-team", [.. Them])]
		};

		Assert.Equal(1, FaceitMatchRows.Backfill([row, other], item));
		Assert.Equal(("their-team", "rival-team", "champ-59"), (row.Team1FactionId, row.Team2FactionId, row.CompetitionId));
		Assert.Null(other.Team1FactionId);
		Assert.Equal(0, FaceitMatchRows.Backfill([row], item));
	}

	#endregion
}
