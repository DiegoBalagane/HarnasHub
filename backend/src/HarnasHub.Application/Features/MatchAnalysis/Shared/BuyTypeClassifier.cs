namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>How much a team invested in a round.</summary>
public enum BuyType
{
	Pistol = 0,
	Eco = 1,
	SemiEco = 2,
	Force = 3,
	Full = 4
}

/// <summary>Pure buy-type classification from a team's total equipment value at the end of the buy phase. Thresholds
/// are per-player averages (so a 4-man team after a disconnect is judged fairly) and roughly match the commonly used
/// team totals of 5k / 12.5k / 19.5k: under $1000 a head is a full save, a cheap pistol + kevlar is a semi-eco, a
/// rifle/SMG without full utility is a force, and a rifle + full armor (≈ $3900 incl. the default pistol) is a full buy.</summary>
public static class BuyTypeClassifier
{
	#region Public Fields

	/// <summary>Rounds per half in regulation (MR12).</summary>
	public const int RegulationHalfLength = 12;

	/// <summary>Per-player average below which a round is an eco.</summary>
	public const int EcoMaxAverage = 1000;

	/// <summary>Per-player average below which a round is a semi-eco.</summary>
	public const int SemiEcoMaxAverage = 2500;

	/// <summary>Per-player average below which a round is a force buy; at or above it is a full buy.</summary>
	public const int ForceMaxAverage = 3900;

	#endregion

	#region Public Methods

	/// <summary>Whether a round is a pistol round: round 1 and the first round of the second half (13) under MR12.
	/// Overtime half openers are deliberately NOT pistol rounds — CS2 resets everyone to $12,500 in overtime, so those
	/// rounds are full buys and classifying them by value is what describes them correctly.</summary>
	public static bool IsPistolRound(int roundNumber) => roundNumber is 1 or RegulationHalfLength + 1;

	/// <summary>Classifies a team's round from its summed equipment value and how many players it had.</summary>
	public static BuyType Classify(int roundNumber, int totalEquipmentValue, int playerCount)
	{
		if (IsPistolRound(roundNumber))
		{
			return BuyType.Pistol;
		}

		var average = totalEquipmentValue / Math.Max(playerCount, 1);

		return average switch
		{
			< EcoMaxAverage => BuyType.Eco,
			< SemiEcoMaxAverage => BuyType.SemiEco,
			< ForceMaxAverage => BuyType.Force,
			_ => BuyType.Full
		};
	}

	#endregion
}
