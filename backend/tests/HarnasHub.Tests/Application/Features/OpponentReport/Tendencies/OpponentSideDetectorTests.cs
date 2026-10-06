#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class OpponentSideDetectorTests
{
	#region Public Methods

	[Fact]
	public void Should_pick_the_team_containing_known_opponent_players()
	{
		var rounds = new[] { Round(1, MapSide.T, MapSide.T), Round(13, MapSide.CT, MapSide.CT) };

		Assert.Equal("A", OpponentSideDetector.Detect(rounds, new HashSet<long> { 11, 12 }));
		Assert.Equal("B", OpponentSideDetector.Detect(rounds, new HashSet<long> { 21 }));
	}

	[Fact]
	public void Should_return_null_without_known_players_or_on_a_tie()
	{
		var rounds = new[] { Round(1, MapSide.T, MapSide.T) };

		Assert.Null(OpponentSideDetector.Detect(rounds, new HashSet<long>()));
		Assert.Null(OpponentSideDetector.Detect(rounds, new HashSet<long> { 99 }));
		Assert.Null(OpponentSideDetector.Detect(rounds, new HashSet<long> { 11, 21 }));
		Assert.Null(OpponentSideDetector.Detect([], new HashSet<long> { 11 }));
	}

	[Fact]
	public void Should_return_round_one_rosters_as_teams_a_and_b()
	{
		var rounds = new[] { Round(2, MapSide.CT, null), Round(1, MapSide.T, null) };

		Assert.Equal(Them, OpponentSideDetector.TeamRoster(rounds, "A"));
		Assert.Equal(Others, OpponentSideDetector.TeamRoster(rounds, "B"));
	}

	#endregion
}
