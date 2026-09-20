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

	/// <summary>A deliberately simple stand-in for HLTV's Rating 2.0 — kills/deaths/assists per round, a damage term,
	/// and a small KAST bonus, clamped at 0. Coach/Manager can edit a manually entered row afterwards, so it only
	/// needs to be a reasonable starting point, not exact.</summary>
	private static double ApproximateRating(DemoPlayerStats player, int roundsPlayed, double adr, double kastPercentage)
	{
		var killsPerRound = (double)player.Kills / roundsPlayed;
		var deathsPerRound = (double)player.Deaths / roundsPlayed;
		var assistsPerRound = (double)player.Assists / roundsPlayed;

		var rating = killsPerRound * 0.45 + assistsPerRound * 0.15 - deathsPerRound * 0.3 + adr / 100 * 0.25 + kastPercentage / 100 * 0.15;

		return Math.Round(Math.Max(rating, 0), 2);
	}

	#endregion

	#region Public Types

	public record ComputedStats(double Adr, double HeadshotPercentage, double KastPercentage, double Rating);

	#endregion
}
