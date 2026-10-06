using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class TeamFormCalculatorTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public void Should_list_the_last_ten_games_newest_first_with_the_streak()
	{
		var games = Enumerable.Range(0, 12)
			.Select(i => Game(won: i >= 3, Now.AddDays(-i), ["a", "b", "c"]))
			.Reverse()
			.ToList();

		var form = TeamFormCalculator.Calculate(games, new Dictionary<string, string>());

		Assert.Equal(TeamFormCalculator.LastGamesCount, form.LastGames.Count);
		Assert.Equal(Now, form.LastGames[0].PlayedAtUtc);
		Assert.Equal("L3", form.Streak);
	}

	[Fact]
	public void Should_name_players_who_only_appear_in_the_latest_games()
	{
		var games = Enumerable.Range(0, 8)
			.Select(i => Game(true, Now.AddDays(-i), i < 5 ? ["a", "b", "new"] : ["a", "b", "old"]))
			.ToList();

		var form = TeamFormCalculator.Calculate(games, new Dictionary<string, string> { ["new"] = "Newbie" });

		Assert.Equal(["Newbie"], form.NewPlayers);
	}

	[Fact]
	public void Should_count_new_players_outside_the_linked_list_instead_of_showing_their_ids()
	{
		var games = Enumerable.Range(0, 8)
			.Select(i => Game(true, Now.AddDays(-i), i < 5 ? ["a", "new", "stand-in"] : ["a", "old", "old2"]))
			.ToList();

		var form = TeamFormCalculator.Calculate(games, new Dictionary<string, string> { ["new"] = "Newbie" });

		Assert.Equal(["Newbie", "1 gracz spoza listy"], form.NewPlayers);
	}

	[Fact]
	public void Should_not_report_lineup_changes_without_enough_history()
	{
		var games = Enumerable.Range(0, 6).Select(i => Game(true, Now.AddDays(-i), [$"p{i}"])).ToList();

		Assert.Empty(TeamFormCalculator.Calculate(games, new Dictionary<string, string>()).NewPlayers);
	}

	[Fact]
	public void Should_have_no_streak_without_games()
	{
		Assert.Null(TeamFormCalculator.Streak([]));
	}

	#endregion

	#region Private Methods

	private static TeamGame Game(bool won, DateTime at, List<string> players) =>
		new(Guid.NewGuid().ToString(), at, MapName.Mirage, "de_mirage", won ? 13 : 7, won ? 7 : 13, won, players, "FACEIT League");

	#endregion
}
