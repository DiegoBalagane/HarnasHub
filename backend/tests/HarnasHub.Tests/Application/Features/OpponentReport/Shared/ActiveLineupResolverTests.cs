using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class ActiveLineupResolverTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_take_the_players_of_the_recent_team_games_out_of_a_16_member_team_page()
	{
		var active = new[] { "a1", "a2", "a3", "a4", "a5", "a6" };
		var roster = active.Concat(Enumerable.Range(0, 10).Select(i => $"ex{i}")).ToHashSet();
		// a1–a4 play every game, a5 and a6 share the fifth slot; ex0 stood in once; ex1–ex3 only last season.
		var games = Enumerable.Range(0, 10)
			.Select(i => Game(Now.AddDays(-i * 3), i == 4 ? ["a1", "a2", "a3", "a4", "ex0"] : ["a1", "a2", "a3", "a4", i % 2 == 0 ? "a5" : "a6"]))
			.Concat(Enumerable.Range(0, 5).Select(i => Game(Now.AddDays(-100 - i), ["ex1", "ex2", "ex3", "a1", "a2"])))
			.ToList();

		var lineup = ActiveLineupResolver.Resolve(games, roster, Now, new Dictionary<string, string> { ["a1"] = "Alpha" }, []);

		Assert.Equal(active, lineup.ActiveIds.Order());
		Assert.Equal(10, lineup.Dto.WindowGames);
		Assert.Contains(lineup.Dto.Active, p => p.PlayerId == "a1" && p.Nickname == "Alpha" && p.RecentTeamGames == 10);
		Assert.Equal(10, lineup.Dto.Inactive.Count);
		Assert.Contains(lineup.Dto.Inactive, p => p.PlayerId == "ex0" && p.RecentTeamGames == 1);
		Assert.Contains(lineup.Dto.Inactive, p => p.PlayerId == "ex1" && p.TeamGames == 5 && p.RecentTeamGames == 0);
		Assert.StartsWith("Skład z ostatnich 10 meczów drużynowych", lineup.Dto.Basis);
	}

	[Fact]
	public void Should_extend_the_window_to_every_team_game_of_the_last_60_days()
	{
		var roster = new[] { "a1", "a2", "a3", "a4", "a5", "b1" }.ToHashSet();
		var games = Enumerable.Range(0, 12).Select(i => Game(Now.AddDays(-i * 4), ["a1", "a2", "a3", "a4", "a5"]))
			.Concat(Enumerable.Range(0, 3).Select(i => Game(Now.AddDays(-50 - i), ["a1", "a2", "a3", "b1", "a5"])))
			.OrderByDescending(g => g.PlayedAtUtc)
			.ToList();

		var lineup = ActiveLineupResolver.Resolve(games, roster, Now, new Dictionary<string, string>(), []);

		Assert.Equal(15, lineup.Dto.WindowGames);
		Assert.Contains("b1", lineup.ActiveIds);
	}

	[Fact]
	public void Should_keep_a_list_of_five_and_fall_back_to_everyone_without_team_games()
	{
		var five = new[] { "a1", "a2", "a3", "a4", "a5" }.ToHashSet();
		var many = five.Concat(["b1", "b2"]).ToHashSet();

		Assert.Equal(5, ActiveLineupResolver.Resolve([], five, Now, new Dictionary<string, string>(), []).ActiveIds.Count);
		var fallback = ActiveLineupResolver.Resolve([], many, Now, new Dictionary<string, string>(), []);
		Assert.Equal(7, fallback.ActiveIds.Count);
		Assert.StartsWith("Brak meczów drużynowych", fallback.Dto.Basis);
	}

	#endregion

	#region Private Methods

	private static TeamGame Game(DateTime playedAtUtc, List<string> side) =>
		new(Guid.NewGuid().ToString(), playedAtUtc, MapName.Mirage, "de_mirage", 13, 8, true, side, "ESEA League");

	#endregion
}
