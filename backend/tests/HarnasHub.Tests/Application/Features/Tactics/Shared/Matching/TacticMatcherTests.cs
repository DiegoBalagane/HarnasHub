#region Usings

using HarnasHub.Application.Features.Tactics.Shared.Matching;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.Shared.Matching;

public class TacticMatcherTests
{
	#region Public Methods

	[Theory]
	[InlineData(0f, 1.0)]
	[InlineData(0.04f, 1.0)]
	[InlineData(0.06f, 0.5)]
	[InlineData(0.08f, 0.0)]
	[InlineData(0.5f, 0.0)]
	public void Should_give_full_credit_inside_the_radius_and_fall_linearly_to_zero(float distance, double expected)
	{
		var credit = TacticMatcher.Credit(distance, TacticMatcher.PositionFullRadius, TacticMatcher.PositionZeroRadius);

		Assert.Equal(expected, credit, 3);
	}

	[Fact]
	public void Should_not_score_a_tactic_of_the_other_side()
	{
		var round = Round(MapSide.T, players: [new(0.5f, 0.5f)]);

		Assert.Null(TacticMatcher.Score(round, Tactic(MapSide.CT, Position(0.5f, 0.5f))));
	}

	[Fact]
	public void Should_average_position_and_grenade_credits()
	{
		var round = Round(MapSide.T, players: [new(0.5f, 0.5f)], grenades: [new(GrenadeType.Smoke, 0.2f, 0.2f)]);
		var tactic = Tactic(MapSide.T, Position(0.5f, 0.5f), Nade(0.2f, 0.2f, GrenadeType.Smoke), Nade(0.9f, 0.9f, GrenadeType.Smoke));

		var score = TacticMatcher.Score(round, tactic);

		Assert.Equal(2.0 / 3.0, score!.Value, 3);
	}

	[Fact]
	public void Should_ignore_grenades_of_a_different_type()
	{
		var round = Round(MapSide.T, grenades: [new(GrenadeType.Flash, 0.2f, 0.2f)]);

		Assert.Equal(0.0, TacticMatcher.Score(round, Tactic(MapSide.T, Nade(0.2f, 0.2f, GrenadeType.Smoke)))!.Value);
		Assert.Equal(1.0, TacticMatcher.Score(round, Tactic(MapSide.T, Nade(0.2f, 0.2f, null)))!.Value);
	}

	[Fact]
	public void Should_skip_position_points_without_samples_and_require_half_of_the_points_evaluable()
	{
		var round = Round(MapSide.T, hasPositions: false, grenades: [new(GrenadeType.Smoke, 0.2f, 0.2f)]);

		Assert.Equal(1.0, TacticMatcher.Score(round, Tactic(MapSide.T, Nade(0.2f, 0.2f, GrenadeType.Smoke), Position(0.5f, 0.5f)))!.Value);
		Assert.Null(TacticMatcher.Score(round, Tactic(MapSide.T, Nade(0.2f, 0.2f, GrenadeType.Smoke), Position(0.5f, 0.5f), Position(0.6f, 0.6f))));
	}

	[Fact]
	public void Should_pick_the_best_tactic_above_the_threshold()
	{
		var round = Round(MapSide.T, players: [new(0.5f, 0.5f), new(0.3f, 0.3f)]);
		var exact = Tactic(MapSide.T, Position(0.5f, 0.5f), Position(0.3f, 0.3f)) with { Name = "Exact" };
		var partial = Tactic(MapSide.T, Position(0.5f, 0.5f), Position(0.9f, 0.9f)) with { Name = "Partial" };

		var match = TacticMatcher.BestMatch(round, [partial, exact]);

		Assert.Equal("Exact", match!.TacticName);
		Assert.Equal(1.0, match.Score, 3);
	}

	[Fact]
	public void Should_leave_a_round_unmatched_below_the_threshold()
	{
		var round = Round(MapSide.T, players: [new(0.5f, 0.5f)]);
		var tactic = Tactic(MapSide.T, Position(0.5f, 0.5f), Position(0.9f, 0.9f));

		Assert.Null(TacticMatcher.BestMatch(round, [tactic]));
	}

	[Fact]
	public void Should_prefer_the_more_specific_tactic_on_a_tie()
	{
		var round = Round(MapSide.T, players: [new(0.5f, 0.5f), new(0.3f, 0.3f)]);
		var small = Tactic(MapSide.T, Position(0.5f, 0.5f)) with { Name = "Small" };
		var large = Tactic(MapSide.T, Position(0.5f, 0.5f), Position(0.3f, 0.3f)) with { Name = "Large" };

		Assert.Equal("Large", TacticMatcher.BestMatch(round, [small, large])!.TacticName);
	}

	#endregion

	#region Private Methods

	private static RoundSignature Round(
		MapSide side, bool hasPositions = true, SignaturePoint[]? players = null, SignatureGrenade[]? grenades = null, bool? won = true) =>
		new(1, side, won, hasPositions, players ?? [], grenades ?? []);

	private static TacticTarget Tactic(MapSide side, params TacticTargetPoint[] points) => new(Guid.NewGuid(), "Tactic", side, points);

	private static TacticTargetPoint Position(float x, float y) => new(x, y, false, null);

	private static TacticTargetPoint Nade(float x, float y, GrenadeType? type) => new(x, y, true, type);

	#endregion
}
