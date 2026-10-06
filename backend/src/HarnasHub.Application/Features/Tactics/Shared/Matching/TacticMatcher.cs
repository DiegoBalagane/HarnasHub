#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared.Matching;

/// <summary>One tactic point as a matching target: a grenade point (linked to a library nade, compared with where our
/// grenades landed, of the same type when known) or a position point (compared with where our players stood).</summary>
public record TacticTargetPoint(float X, float Y, bool IsGrenade, GrenadeType? GrenadeType);

/// <summary>A Playbook tactic reduced to what matching needs.</summary>
public record TacticTarget(Guid TacticId, string Name, MapSide Side, IReadOnlyList<TacticTargetPoint> Points);

/// <summary>The tactic a round was matched to, with its score in [0,1] and whether we won the round.</summary>
public record TacticRoundMatch(int RoundNumber, MapSide Side, bool? WeWon, Guid TacticId, string TacticName, double Score);

/// <summary>Pure, explainable matching of our rounds to Playbook tactics. Every tactic point earns credit by distance to the
/// nearest evidence: full credit within the "full" radius, falling linearly to zero at the "zero" radius (radar fractions).
/// A tactic's score is the mean credit over the points that could be evaluated (position points can't be without position
/// samples); at least half of its points must be evaluable. The best tactic of the round's side wins if it reaches
/// <see cref="MatchThreshold"/>; ties go to the tactic with more points (more specific).</summary>
public static class TacticMatcher
{
	#region Public Fields

	/// <summary>Minimum score for a round to count as played with a tactic.</summary>
	public const double MatchThreshold = 0.6;

	/// <summary>A grenade landing this close (radar fraction) to a grenade point earns full credit.</summary>
	public const float GrenadeFullRadius = 0.03f;

	/// <summary>A grenade landing this far or further earns no credit.</summary>
	public const float GrenadeZeroRadius = 0.07f;

	/// <summary>A player this close to a position point earns full credit.</summary>
	public const float PositionFullRadius = 0.04f;

	/// <summary>A player this far or further earns no credit.</summary>
	public const float PositionZeroRadius = 0.08f;

	/// <summary>Share of a tactic's points that must be evaluable for it to be scored at all.</summary>
	public const double MinEvaluableShare = 0.5;

	#endregion

	#region Public Methods

	/// <summary>Credit in [0,1] for a distance: 1 up to <paramref name="fullRadius"/>, linear to 0 at <paramref name="zeroRadius"/>.</summary>
	public static double Credit(float distance, float fullRadius, float zeroRadius) =>
		distance <= fullRadius ? 1
		: distance >= zeroRadius ? 0
		: 1 - (distance - fullRadius) / (zeroRadius - fullRadius);

	/// <summary>How well <paramref name="round"/> fits <paramref name="tactic"/>, or null when it can't be judged (other
	/// side, no points, or too few evaluable points).</summary>
	public static double? Score(RoundSignature round, TacticTarget tactic)
	{
		if (tactic.Side != round.OurSide || tactic.Points.Count == 0)
		{
			return null;
		}

		var total = 0.0;
		var evaluated = 0;
		foreach (var point in tactic.Points)
		{
			if (point.IsGrenade)
			{
				var nearest = round.Grenades
					.Where(g => point.GrenadeType is null || g.Type == point.GrenadeType)
					.Select(g => Distance(point.X, point.Y, g.X, g.Y))
					.DefaultIfEmpty(float.MaxValue)
					.Min();
				total += Credit(nearest, GrenadeFullRadius, GrenadeZeroRadius);
				evaluated++;
			}
			else if (round.HasPositions)
			{
				var nearest = round.PlayerPoints
					.Select(p => Distance(point.X, point.Y, p.X, p.Y))
					.DefaultIfEmpty(float.MaxValue)
					.Min();
				total += Credit(nearest, PositionFullRadius, PositionZeroRadius);
				evaluated++;
			}
		}

		if (evaluated == 0 || evaluated < tactic.Points.Count * MinEvaluableShare)
		{
			return null;
		}

		return total / evaluated;
	}

	/// <summary>The best tactic for the round at or above <see cref="MatchThreshold"/>, or null.</summary>
	public static TacticRoundMatch? BestMatch(RoundSignature round, IReadOnlyList<TacticTarget> tactics)
	{
		TacticRoundMatch? best = null;
		var bestPoints = 0;

		foreach (var tactic in tactics)
		{
			if (Score(round, tactic) is not { } score || score < MatchThreshold)
			{
				continue;
			}

			var better = best is null
				|| score > best.Score + 1e-9
				|| (Math.Abs(score - best.Score) <= 1e-9 && tactic.Points.Count > bestPoints);
			if (better)
			{
				best = new TacticRoundMatch(round.RoundNumber, round.OurSide, round.WeWon, tactic.TacticId, tactic.Name, score);
				bestPoints = tactic.Points.Count;
			}
		}

		return best;
	}

	/// <summary>Matches every round; rounds without a match are left out.</summary>
	public static List<TacticRoundMatch> MatchAll(IEnumerable<RoundSignature> rounds, IReadOnlyList<TacticTarget> tactics) =>
		rounds.Select(r => BestMatch(r, tactics)).OfType<TacticRoundMatch>().ToList();

	#endregion

	#region Private Methods

	private static float Distance(float x1, float y1, float x2, float y2)
	{
		var dx = x1 - x2;
		var dy = y1 - y2;
		return MathF.Sqrt(dx * dx + dy * dy);
	}

	#endregion
}
