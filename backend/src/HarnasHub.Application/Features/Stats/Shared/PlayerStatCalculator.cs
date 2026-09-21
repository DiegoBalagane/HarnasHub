using HarnasHub.Application.Abstractions;

namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>Turns one player's raw demo totals into the derived numbers stored on a <c>PlayerMatchStat</c> row —
/// shared between <c>ImportStatsFromDemo</c> and <c>AddResult</c> (which imports stats for matched roster members
/// straight off an attached demo), so the two paths can never drift apart on the formula.</summary>
public static class PlayerStatCalculator
{
	#region Public Methods

	public static ComputedStats Compute(DemoPlayerStats player, int roundsPlayed)
	{
		var adr = Math.Round((double)player.DamageDealt / roundsPlayed, 1);
		var headshotPercentage = player.Kills == 0 ? 0 : Math.Round((double)player.Headshots / player.Kills * 100, 1);
		var kastPercentage = Math.Round((double)player.KastRounds / roundsPlayed * 100, 1);
		var rating = ApproximateRating(player, roundsPlayed, adr, kastPercentage);

		return new ComputedStats(adr, headshotPercentage, kastPercentage, rating);
	}

	#endregion

	#region Private Methods

	/// <summary>A stand-in for HLTV's Rating 2.0, using the community-reverse-engineered approximation of its published
	/// formula (HLTV has never disclosed the exact weights) — KAST/KPR/DPR/Impact/ADR terms plus a baseline constant
	/// that centers an average performance around 1.00, the same way the real thing does. Coach/Manager can edit a
	/// manually entered row afterwards, so it only needs to land in the right ballpark, not be exact.</summary>
	private static double ApproximateRating(DemoPlayerStats player, int roundsPlayed, double adr, double kastPercentage)
	{
		var killsPerRound = (double)player.Kills / roundsPlayed;
		var deathsPerRound = (double)player.Deaths / roundsPlayed;
		var assistsPerRound = (double)player.Assists / roundsPlayed;

		var impact = 2.13 * killsPerRound + 0.42 * assistsPerRound - 0.41;

		var rating =
			0.0073 * kastPercentage
			+ 0.3591 * killsPerRound
			- 0.5329 * deathsPerRound
			+ 0.2372 * impact
			+ 0.0032 * adr
			+ 0.1587;

		return Math.Round(Math.Max(rating, 0), 2);
	}

	#endregion

	#region Public Types

	public record ComputedStats(double Adr, double HeadshotPercentage, double KastPercentage, double Rating);

	#endregion
}
