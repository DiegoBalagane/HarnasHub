using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>Works out a match score from a parsed demo's rounds by spotting which side our roster stood on in each one.</summary>
public static class DemoScoreCalculator
{
	#region Public Methods

	/// <summary>Returns our/opponent round wins, or null when not a single round could be attributed to our roster
	/// (nobody in the demo has a SteamID64 on file yet) — the caller then has to fall back to a manually entered score.</summary>
	public static (int OurScore, int OpponentScore)? Calculate(IReadOnlyList<DemoRoundResult> rounds, IReadOnlySet<long> ourRosterSteamIds)
	{
		var ourScore = 0;
		var opponentScore = 0;
		var attributedRounds = 0;

		foreach (var round in rounds)
		{
			// Re-decided every round rather than once per demo: both teams swap sides at halftime and again in
			// overtime, and a full five-man roster always swaps together, so the majority match stays reliable.
			var terroristMatches = round.TerroristSteamIds.Count(ourRosterSteamIds.Contains);
			var counterTerroristMatches = round.CounterTerroristSteamIds.Count(ourRosterSteamIds.Contains);

			if (terroristMatches == 0 && counterTerroristMatches == 0)
			{
				continue;
			}

			var ourSide = terroristMatches >= counterTerroristMatches ? MapSide.T : MapSide.CT;
			attributedRounds++;

			if (round.WinnerSide == ourSide)
			{
				ourScore++;
			}
			else
			{
				opponentScore++;
			}
		}

		return attributedRounds == 0 ? null : (ourScore, opponentScore);
	}

	#endregion
}
