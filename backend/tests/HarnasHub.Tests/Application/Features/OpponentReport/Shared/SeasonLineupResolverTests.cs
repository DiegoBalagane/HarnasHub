using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class SeasonLineupResolverTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 7, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_pick_the_highest_season_even_when_an_older_one_has_more_recent_playoff_dates()
	{
		// Playoffs of S58 dated after the first S59 match (FACEIT overlaps them) must not win.
		var games = new List<TeamGame>
		{
			Game("p58", Now.AddDays(-1), 58, ["a1", "a2", "a3", "a4", "a5"]),
			Game("r59", Now.AddDays(-3), 59, ["a1", "a2", "a3", "n1", "n2"]),
			Game("r58", Now.AddDays(-30), 58, ["a1", "a2", "a3", "a4", "a5"])
		};

		var lineup = SeasonLineupResolver.Resolve(games, new HashSet<string> { "a1", "a2", "a3", "a4", "a5" }, new Dictionary<string, string> { ["n1"] = "Newbie" }, []);

		Assert.Equal(59, SeasonLineupResolver.CurrentSeason(games));
		Assert.NotNull(lineup);
		Assert.Equal(["a1", "a2", "a3", "n1", "n2"], lineup.ActiveIds.Order());
		Assert.Equal(1, lineup.Dto.WindowGames);
		Assert.Contains(lineup.Dto.Active, p => p.PlayerId == "n1" && p.Nickname == "Newbie" && p.RecentTeamGames == 1);
		Assert.Equal(["a4", "a5"], lineup.Dto.Inactive.Select(p => p.PlayerId).Order());
	}

	[Fact]
	public void Should_return_null_without_esea_games()
	{
		var games = new List<TeamGame> { Game("cup", Now, null, ["a1", "a2", "a3", "a4", "a5"]) };

		Assert.Null(SeasonLineupResolver.CurrentSeason(games));
		Assert.Null(SeasonLineupResolver.Resolve(games, new HashSet<string>(), new Dictionary<string, string>(), []));
	}

	#endregion

	#region Private Methods

	private static TeamGame Game(string matchId, DateTime playedAtUtc, int? season, List<string> side) =>
		new(matchId, playedAtUtc, MapName.Mirage, "de_mirage", 13, 8, true, side, season is null ? "Cup" : $"S{season} EU Main A")
		{
			Official = true,
			Season = season
		};

	#endregion
}
