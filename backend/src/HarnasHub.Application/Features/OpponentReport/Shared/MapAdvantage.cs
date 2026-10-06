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

	/// <summary><c>(wins + k·0.5) / (games + k)</c>; exactly 0.5 without games. Draws may be passed as half wins.</summary>
	public static double SmoothedWinRate(double wins, int games, double k = SmoothingGames) =>
		(wins + k * 0.5) / (games + k);

	/// <summary>Smoothed win rate of ours minus theirs, in the range −1…1.</summary>
	public static double Advantage(double ourWins, int ourGames, double theirWins, int theirGames) =>
		SmoothedWinRate(ourWins, ourGames) - SmoothedWinRate(theirWins, theirGames);

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
