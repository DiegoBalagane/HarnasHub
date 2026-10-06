namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Short Polish notes about the opponent's side of a map, used in the simulated veto step reasons (our side comes from
/// the veto score's own note, which names the real driver).</summary>
public static class VetoNotes
{
	#region Public Methods

	/// <summary>Their side of a map in a few words; the win rate is only quoted from <see cref="SampleThresholds.MinGamesForWinRate"/> games.</summary>
	public static string Theirs(MapMetrics their) =>
		their.Games == 0
			? "oni: nie grają"
			: SampleThresholds.HasWinRateSample(their.Games)
				? $"oni: {their.Games} {MatchNoun(their.Games)}, {Math.Round((their.WinRate ?? 0) * 100, 1):0}% wygranych"
				: $"oni: {their.Games} {MatchNoun(their.Games)} (za mało na % wygranych)";

	/// <summary>Polish plural of "mecz": 1 mecz, 2–4 mecze (except 12–14), otherwise meczów.</summary>
	public static string MatchNoun(int count)
	{
		if (count == 1)
		{
			return "mecz";
		}

		var lastDigit = count % 10;
		var lastTwo = count % 100;
		return lastDigit is >= 2 and <= 4 && lastTwo is < 12 or > 14 ? "mecze" : "meczów";
	}

	#endregion
}
