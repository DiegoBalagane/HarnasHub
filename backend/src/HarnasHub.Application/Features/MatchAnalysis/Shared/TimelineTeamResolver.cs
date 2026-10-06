#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Pure "which side were we" logic for a timeline. A demo never labels either team as ours, so "us" is a set of
/// SteamID64s and each round's side is the one most of them stood on — the same per-round majority rule as
/// <c>DemoScoreCalculator</c>, which survives halftime/overtime swaps and a substitute or two.</summary>
public static class TimelineTeamResolver
{
	#region Public Methods

	/// <summary>The side our players stood on in <paramref name="round"/>, or null when none of them played it.</summary>
	public static MapSide? OurSide(DemoTimelineRound round, IReadOnlySet<long> ourSteamIds)
	{
		var onT = round.TerroristSteamIds.Count(ourSteamIds.Contains);
		var onCt = round.CounterTerroristSteamIds.Count(ourSteamIds.Contains);

		if (onT == 0 && onCt == 0)
		{
			return null;
		}

		return onT >= onCt ? MapSide.T : MapSide.CT;
	}

	/// <summary>Picks our team's round-1 SteamID64s: the round-1 side overlapping the roster wins; failing that, the side
	/// whose would-be score equals the result's recorded score (when exactly one does); otherwise empty (unknown).</summary>
	public static IReadOnlyList<long> ResolveOurTeam(
		IReadOnlyList<DemoTimelineRound> rounds,
		IReadOnlySet<long> rosterSteamIds,
		int? ourScore,
		int? opponentScore)
	{
		if (rounds.Count == 0)
		{
			return [];
		}

		var first = rounds.OrderBy(r => r.Number).First();
		var startedT = first.TerroristSteamIds;
		var startedCt = first.CounterTerroristSteamIds;

		var rosterOnT = startedT.Count(rosterSteamIds.Contains);
		var rosterOnCt = startedCt.Count(rosterSteamIds.Contains);
		if (rosterOnT != rosterOnCt)
		{
			return rosterOnT > rosterOnCt ? startedT : startedCt;
		}

		if (ourScore is not { } us || opponentScore is not { } them)
		{
			return [];
		}

		var tScore = Score(rounds, startedT.ToHashSet());
		var ctScore = Score(rounds, startedCt.ToHashSet());
		var tMatches = tScore == (us, them);
		var ctMatches = ctScore == (us, them);

		return tMatches == ctMatches ? [] : tMatches ? startedT : startedCt;
	}

	/// <summary>Round wins/losses of the team identified by <paramref name="teamSteamIds"/>; rounds without a winner or
	/// without any of its players are skipped.</summary>
	public static (int Wins, int Losses) Score(IReadOnlyList<DemoTimelineRound> rounds, IReadOnlySet<long> teamSteamIds)
	{
		var wins = 0;
		var losses = 0;

		foreach (var round in rounds)
		{
			if (round.WinnerSide is not { } winner || OurSide(round, teamSteamIds) is not { } side)
			{
				continue;
			}

			if (winner == side)
			{
				wins++;
			}
			else
			{
				losses++;
			}
		}

		return (wins, losses);
	}

	#endregion
}
