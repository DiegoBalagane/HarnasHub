using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.Shared;

public class DemoScoreCalculatorTests
{
	#region Private Fields

	private static readonly long[] OurRoster = [1, 2, 3, 4, 5];
	private static readonly long[] TheirRoster = [11, 12, 13, 14, 15];

	#endregion

	#region Public Methods

	[Fact]
	public void Should_return_null_when_there_are_no_rounds()
	{
		var score = DemoScoreCalculator.Calculate([], OurRoster.ToHashSet());

		Assert.Null(score);
	}

	[Fact]
	public void Should_return_null_when_no_roster_member_appears_in_any_round()
	{
		IReadOnlyList<DemoRoundResult> rounds = [Round(MapSide.T, TheirRoster, [21, 22, 23])];

		var score = DemoScoreCalculator.Calculate(rounds, new HashSet<long>());

		Assert.Null(score);
	}

	[Fact]
	public void Should_count_a_round_won_on_our_current_side_as_ours()
	{
		IReadOnlyList<DemoRoundResult> rounds = [Round(MapSide.T, OurRoster, TheirRoster)];

		var score = DemoScoreCalculator.Calculate(rounds, OurRoster.ToHashSet());

		Assert.Equal((1, 0), score);
	}

	[Fact]
	public void Should_follow_the_roster_across_a_halftime_side_swap()
	{
		// First half on T (2 wins of 3), second half on CT after the swap (1 win of 3) — 3:3 overall.
		IReadOnlyList<DemoRoundResult> rounds =
		[
			Round(MapSide.T, OurRoster, TheirRoster),
			Round(MapSide.T, OurRoster, TheirRoster),
			Round(MapSide.CT, OurRoster, TheirRoster),
			Round(MapSide.T, TheirRoster, OurRoster),
			Round(MapSide.T, TheirRoster, OurRoster),
			Round(MapSide.CT, TheirRoster, OurRoster)
		];

		var score = DemoScoreCalculator.Calculate(rounds, OurRoster.ToHashSet());

		Assert.Equal((3, 3), score);
	}

	[Fact]
	public void Should_pick_the_side_holding_more_of_our_roster_when_a_player_appears_on_both()
	{
		// A stand-in who played for the opponent earlier still leaves our five-man majority on T.
		IReadOnlyList<DemoRoundResult> rounds = [Round(MapSide.T, OurRoster, [11, 12, 13, 14, 5])];

		var score = DemoScoreCalculator.Calculate(rounds, OurRoster.ToHashSet());

		Assert.Equal((1, 0), score);
	}

	[Fact]
	public void Should_skip_rounds_with_no_roster_member_on_either_side()
	{
		IReadOnlyList<DemoRoundResult> rounds =
		[
			Round(MapSide.CT, [21, 22], [23, 24]),
			Round(MapSide.CT, TheirRoster, OurRoster)
		];

		var score = DemoScoreCalculator.Calculate(rounds, OurRoster.ToHashSet());

		Assert.Equal((1, 0), score);
	}

	#endregion

	#region Private Methods

	private static DemoRoundResult Round(MapSide winnerSide, IReadOnlyList<long> terrorists, IReadOnlyList<long> counterTerrorists) =>
		new(winnerSide, terrorists, counterTerrorists);

	#endregion
}
