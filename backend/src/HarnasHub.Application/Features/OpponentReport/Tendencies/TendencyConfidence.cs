namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>How much a tendency can be trusted, from how many rounds it is based on — same "Low"/"Medium"/"High" vocabulary
/// as the FACEIT map confidence, but counted in rounds (a single demo already gives ~12 rounds per side).</summary>
public static class TendencyConfidence
{
	#region Public Fields

	/// <summary>Rounds needed for medium confidence.</summary>
	public const int MediumFrom = 10;

	/// <summary>Rounds needed for high confidence.</summary>
	public const int HighFrom = 25;

	#endregion

	#region Public Methods

	/// <summary>"Low", "Medium" or "High" for a sample of <paramref name="rounds"/> rounds.</summary>
	public static string For(int rounds) => rounds >= HighFrom ? "High" : rounds >= MediumFrom ? "Medium" : "Low";

	/// <summary>Share in percent rounded to one decimal; 0 for an empty sample.</summary>
	public static double Percent(int count, int total) => total == 0 ? 0 : Math.Round(100.0 * count / total, 1);

	#endregion
}
