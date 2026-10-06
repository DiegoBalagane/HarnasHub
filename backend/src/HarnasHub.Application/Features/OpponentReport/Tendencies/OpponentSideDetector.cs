#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Which of a demo's two teams is the opponent, from SteamID64s known to belong to them (their FACEIT players and
/// earlier confirmed demos). Teams are named by round 1: "A" started T, "B" started CT. Votes are summed per round with
/// the same majority rule as <see cref="TimelineTeamResolver"/>, so a late substitute or a missing round-1 player doesn't
/// break detection.</summary>
public static class OpponentSideDetector
{
	#region Public Fields

	/// <summary>Name of the team that started on T.</summary>
	public const string TeamA = "A";

	/// <summary>Name of the team that started on CT.</summary>
	public const string TeamB = "B";

	#endregion

	#region Public Methods

	/// <summary>The opponent team ("A"/"B"), or null when none or both teams contain known opponent players equally.</summary>
	public static string? Detect(IReadOnlyList<DemoTimelineRound> rounds, IReadOnlySet<long> knownOpponentIds)
	{
		if (rounds.Count == 0 || knownOpponentIds.Count == 0)
		{
			return null;
		}

		var teamA = TeamRoster(rounds, TeamA).ToHashSet();
		var votesA = 0;
		var votesB = 0;

		foreach (var round in rounds)
		{
			if (TimelineTeamResolver.OurSide(round, teamA) is not { } aSide)
			{
				continue;
			}

			var aIds = aSide == MapSide.T ? round.TerroristSteamIds : round.CounterTerroristSteamIds;
			var bIds = aSide == MapSide.T ? round.CounterTerroristSteamIds : round.TerroristSteamIds;
			votesA += aIds.Count(knownOpponentIds.Contains);
			votesB += bIds.Count(knownOpponentIds.Contains);
		}

		return votesA == votesB ? null : votesA > votesB ? TeamA : TeamB;
	}

	/// <summary>Round-1 SteamID64s of team "A" (started T) or "B" (started CT); empty without rounds.</summary>
	public static IReadOnlyList<long> TeamRoster(IReadOnlyList<DemoTimelineRound> rounds, string team)
	{
		var first = rounds.OrderBy(r => r.Number).FirstOrDefault();
		if (first is null)
		{
			return [];
		}

		return team == TeamA ? first.TerroristSteamIds : first.CounterTerroristSteamIds;
	}

	#endregion
}
