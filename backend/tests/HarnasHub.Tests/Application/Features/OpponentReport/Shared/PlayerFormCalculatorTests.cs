using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.IndividualLineFactory;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class PlayerFormCalculatorTests
{
	#region Public Methods

	[Fact]
	public void Should_compute_per_map_numbers_with_a_team_solo_split()
	{
		var lines = Lines("p1", MapName.Mirage, 4, wins: 3, team: true, kills: 30, deaths: 20)
			.Concat(Lines("p1", MapName.Mirage, 2, wins: 0, kills: 10, deaths: 20, daysAgo: 10))
			.Concat(Lines("p1", MapName.Nuke, 2, wins: 2, daysAgo: 20))
			.Concat(Lines("p1", null, 2, daysAgo: 30))
			.ToList();

		var player = Assert.Single(PlayerFormCalculator.Calculate(lines, [new FaceitPlayerDto("p1", "old", 2100, 9)]));

		Assert.Equal(("nick-p1", 10, 4, 6), (player.Nickname, player.Games, player.TeamGames, player.SoloGames));
		Assert.Equal(2100, player.Elo);
		Assert.Equal(9, player.SkillLevel);
		Assert.Equal(["Mirage", "Nuke"], player.Maps.Select(m => m.MapName));
		var mirage = player.Maps[0];
		Assert.Equal((6, 60.0, 3, 50.0, 1.17), (mirage.Games, mirage.Share, mirage.Wins, mirage.WinRate, mirage.KdRatio));
		Assert.Equal((4, 2), (mirage.TeamGames, mirage.SoloGames));
		Assert.Equal(75, mirage.TeamWinRate);
		Assert.Equal(0, mirage.SoloWinRate);
		Assert.Equal(1.5, mirage.TeamKdRatio);
		Assert.Equal(0.5, mirage.SoloKdRatio);
	}

	[Fact]
	public void Should_list_linked_players_without_games()
	{
		var ghost = Assert.Single(PlayerFormCalculator.Calculate([], [new FaceitPlayerDto("p9", "Ghost", null, null)]));

		Assert.Equal(("Ghost", 0), (ghost.Nickname, ghost.Games));
		Assert.Null(ghost.KdRatio);
		Assert.Null(ghost.RecentForm);
		Assert.Empty(ghost.Maps);
	}

	[Fact]
	public void RecentForm_should_point_up_when_the_last_ten_games_are_clearly_better()
	{
		var newestFirst = Lines("p1", MapName.Mirage, 10, wins: 7, kills: 29, deaths: 20)
			.Concat(Lines("p1", MapName.Mirage, 10, wins: 4, kills: 21, deaths: 20, daysAgo: 20))
			.ToList();

		var form = PlayerFormCalculator.RecentForm(newestFirst)!;

		Assert.Equal((1.45, 1.05, 0.4, "Up"), (form.RecentKdRatio, form.EarlierKdRatio, form.KdDelta, form.Direction));
		Assert.Equal(30, form.WinRateDelta);
	}

	[Fact]
	public void RecentForm_should_be_flat_below_the_threshold_and_null_without_enough_earlier_games()
	{
		var flat = Lines("p1", MapName.Mirage, 10, kills: 22, deaths: 20).Concat(Lines("p1", MapName.Mirage, 5, daysAgo: 20)).ToList();
		var down = Lines("p1", MapName.Mirage, 10, kills: 16, deaths: 20).Concat(Lines("p1", MapName.Mirage, 5, daysAgo: 20)).ToList();
		var tooFew = Lines("p1", MapName.Mirage, 10).Concat(Lines("p1", MapName.Mirage, 4, daysAgo: 20)).ToList();

		Assert.Equal("Flat", PlayerFormCalculator.RecentForm(flat)!.Direction);
		Assert.Equal("Down", PlayerFormCalculator.RecentForm(down)!.Direction);
		Assert.Null(PlayerFormCalculator.RecentForm(tooFew));
	}

	#endregion
}
