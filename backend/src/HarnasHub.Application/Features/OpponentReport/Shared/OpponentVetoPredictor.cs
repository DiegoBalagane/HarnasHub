using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The opponent's expected stance on a map: <paramref name="Prediction"/> is "Ban", "Pick", "Neutral" or "Unknown";
/// <paramref name="Preference"/> ranks maps for the veto simulation (higher = they like it more, 0 = never played).</summary>
public record VetoPrediction(MapName Map, string Prediction, string Reason, double Preference);

/// <summary>Guesses the opponent's veto from what they play — FACEIT doesn't expose ban order, only the map that was played.</summary>
public static class OpponentVetoPredictor
{
	#region Public Fields

	/// <summary>Below this many team games there is too little to call bans and picks.</summary>
	public const int MinGamesForPrediction = 5;

	/// <summary>A map played at most this many times counts as "they don't play it".</summary>
	public const int RarelyPlayedGames = 1;

	/// <summary>How many of their most played maps are flagged as likely picks.</summary>
	public const int LikelyPickCount = 2;

	#endregion

	#region Public Methods

	/// <summary>A prediction for every map in <paramref name="theirs"/>, given <paramref name="totalGames"/> team games overall.</summary>
	public static Dictionary<MapName, VetoPrediction> Predict(IReadOnlyDictionary<MapName, MapMetrics> theirs, int totalGames)
	{
		var likelyPicks = theirs.Values
			.Where(m => m.Games > RarelyPlayedGames && m.SmoothedWinRate >= 0.5)
			.OrderByDescending(m => m.Games)
			.ThenByDescending(m => m.SmoothedWinRate)
			.Take(LikelyPickCount)
			.Select(m => m.Map)
			.ToHashSet();

		return theirs.Values.ToDictionary(m => m.Map, m =>
		{
			var preference = Preference(m);

			if (totalGames < MinGamesForPrediction)
			{
				return new VetoPrediction(m.Map, "Unknown", $"Za mało meczów drużynowych ({totalGames}), by przewidzieć ich veto", preference);
			}

			if (m.Games <= RarelyPlayedGames)
			{
				return new VetoPrediction(m.Map, "Ban", $"Prawie jej nie grają ({m.Games} z {totalGames} meczów) — prawdopodobny ban", preference);
			}

			return likelyPicks.Contains(m.Map)
				? new VetoPrediction(m.Map, "Pick", $"Jedna z ich ulubionych map ({Percent(m.Share)}% meczów, {Percent(m.WinRate ?? 0)}% wygranych) — prawdopodobny pick", preference)
				: new VetoPrediction(m.Map, "Neutral", $"Grają ją okazjonalnie ({m.Games} meczów, {Percent(m.WinRate ?? 0)}% wygranych)", preference);
		});
	}

	/// <summary>How much they like a map: share of their games plus smoothed win rate; 0 for a map they never play.</summary>
	public static double Preference(MapMetrics metrics) =>
		metrics.Games == 0 ? 0 : metrics.Share + metrics.SmoothedWinRate;

	#endregion

	#region Private Methods

	/// <summary>A fraction as a whole percentage.</summary>
	private static int Percent(double fraction) => (int)Math.Round(fraction * 100);

	#endregion
}
