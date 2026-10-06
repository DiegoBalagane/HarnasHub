namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>How much a number based on a small sample can be trusted.</summary>
public enum ConfidenceLevel
{
	/// <summary>Fewer than 3 games on the thinner side.</summary>
	Low = 0,
	/// <summary>3–9 games.</summary>
	Medium = 1,
	/// <summary>10 or more games.</summary>
	High = 2
}

/// <summary>Small-sample-safe win rates and the "our edge over them" number built from them.</summary>
public static class MapAdvantage
{
	#region Public Fields

	/// <summary>Pseudo-games pulled towards 50% — with k = 5, two wins out of two read as ~64%, not 100%.</summary>
	public const double SmoothingGames = 5;

	#endregion

	#region Public Methods

	/// <summary><c>(wins + k·prior) / (games + k)</c>; exactly the prior (0.5 by default) without games. Draws may be passed as half wins.</summary>
	public static double SmoothedWinRate(double wins, double games, double k = SmoothingGames, double prior = 0.5) =>
		(wins + k * prior) / (games + k);

	/// <summary>Smoothed win rate of ours minus theirs, in the range −1…1. The priors default to 0.5; the report passes each side's
	/// solo-form prior from <see cref="IndividualSignal.WinRatePrior"/> (capped to 0.4–0.6), whose weight shrinks as k / (k + games).</summary>
	public static double Advantage(double ourWins, double ourGames, double theirWins, double theirGames, double ourPrior = 0.5, double theirPrior = 0.5) =>
		SmoothedWinRate(ourWins, ourGames, prior: ourPrior) - SmoothedWinRate(theirWins, theirGames, prior: theirPrior);

	/// <summary>Confidence from the smaller of the two samples: &lt; 3 low, 3–9 medium, ≥ 10 high.</summary>
	public static ConfidenceLevel Confidence(int minGames) =>
		minGames < 3 ? ConfidenceLevel.Low : minGames < 10 ? ConfidenceLevel.Medium : ConfidenceLevel.High;

	/// <summary>How strongly an advantage of this confidence should move a veto score.</summary>
	public static double VetoWeight(ConfidenceLevel confidence) => confidence switch
	{
		ConfidenceLevel.High => 0.8,
		ConfidenceLevel.Medium => 0.5,
		_ => 0.25
	};

	#endregion
}
