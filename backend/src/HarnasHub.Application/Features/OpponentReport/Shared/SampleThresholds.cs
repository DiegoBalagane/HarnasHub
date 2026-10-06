namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Minimum evidence before a number may drive a recommendation or a claim — below it the number is shown, flagged as
/// "za mało danych", and left out of the decision.</summary>
public static class SampleThresholds
{
	#region Public Fields

	/// <summary>Games on a map (ours: internal + FACEIT team games; theirs: team games) before a win rate counts or is quoted.</summary>
	public const int MinGamesForWinRate = 5;

	/// <summary>Opponent team games overall before "they (almost) never play X → ban" may be claimed from team games alone.</summary>
	public const int MinTeamGamesForAvoidance = 15;

	#endregion

	#region Public Methods

	/// <summary>Whether a per-map sample is big enough for its win rate to count.</summary>
	public static bool HasWinRateSample(int games) => games >= MinGamesForWinRate;

	#endregion
}
