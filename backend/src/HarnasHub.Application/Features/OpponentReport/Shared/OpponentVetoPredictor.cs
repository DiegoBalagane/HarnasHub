using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The opponent's expected stance on a map: <paramref name="Prediction"/> is "Ban", "Pick", "Neutral" or "Unknown";
/// <paramref name="Preference"/> ranks maps for the veto simulation (higher = they like it more, 0 = never played).</summary>
public record VetoPrediction(MapName Map, string Prediction, string Reason, double Preference);

/// <summary>Guesses the opponent's veto from what they play — FACEIT doesn't expose ban order, only the map that was played.
/// Team games decide; the players' solo comfort (<see cref="MapComfort"/>) is blended in with <see cref="IndividualSignal"/>
/// shrinkage, so it matters with few team games and fades away with many.</summary>
public static class OpponentVetoPredictor
{
	#region Public Fields

	/// <summary>Below this many team games there is too little to call bans and picks from team games alone.</summary>
	public const int MinGamesForPrediction = 5;

	/// <summary>A map played at most this many times counts as "they don't play it".</summary>
	public const int RarelyPlayedGames = 1;

	/// <summary>How many of their most played maps are flagged as likely picks.</summary>
	public const int LikelyPickCount = 2;

	/// <summary>A map with at most this many team games that most players also avoid solo is upgraded to a likely ban.</summary>
	public const int SoloBanMaxTeamGames = 2;

	#endregion

	#region Public Methods

	/// <summary>A prediction for every map in <paramref name="theirs"/>, given <paramref name="totalGames"/> team games overall and
	/// optionally the players' solo <paramref name="comfort"/> per map.</summary>
	public static Dictionary<MapName, VetoPrediction> Predict(
		IReadOnlyDictionary<MapName, MapMetrics> theirs,
		int totalGames,
		IReadOnlyDictionary<MapName, MapComfort>? comfort = null)
	{
		var usable = theirs.Keys
			.Select(map => comfort?.GetValueOrDefault(map))
			.Where(IndividualSignal.IsUsable)
			.ToDictionary(c => c!.Map, c => c!);
		var likelyPicks = theirs.Values
			.Where(m => m.Games > RarelyPlayedGames && m.SmoothedWinRate >= 0.5)
			.OrderByDescending(m => m.Games)
			.ThenByDescending(m => m.SmoothedWinRate)
			.Take(LikelyPickCount)
			.Select(m => m.Map)
			.ToHashSet();
		var soloPicks = usable.Values
			.Where(c => c.IsComfortable)
			.OrderByDescending(IndividualSignal.Preference)
			.Take(LikelyPickCount)
			.Select(c => c.Map)
			.ToHashSet();

		return theirs.Values.ToDictionary(m => m.Map, m =>
		{
			var solo = usable.GetValueOrDefault(m.Map);
			var preference = IndividualSignal.BlendPreference(Preference(m), m.Games, totalGames, theirs.Count, solo);

			return totalGames < MinGamesForPrediction
				? FewTeamGames(m, totalGames, solo, soloPicks.Contains(m.Map), preference)
				: FromTeamGames(m, totalGames, solo, likelyPicks.Contains(m.Map), preference);
		});
	}

	/// <summary>How much they like a map from team games: share of their games plus smoothed win rate; 0 for a map they never play.</summary>
	public static double Preference(MapMetrics metrics) =>
		metrics.Games == 0 ? 0 : metrics.Share + metrics.SmoothedWinRate;

	#endregion

	#region Private Methods

	/// <summary>Enough team games: the original rules, plus a ban upgrade for a barely played map that most players avoid solo too.</summary>
	private static VetoPrediction FromTeamGames(MapMetrics m, int totalGames, MapComfort? solo, bool likelyPick, double preference)
	{
		var note = SoloNote(solo);

		if (m.Games <= RarelyPlayedGames)
		{
			return new VetoPrediction(m.Map, "Ban", $"Prawie jej nie grają ({m.Games} z {totalGames} meczów) — prawdopodobny ban{note}", preference);
		}

		if (likelyPick)
		{
			return new VetoPrediction(m.Map, "Pick", $"Jedna z ich ulubionych map ({Percent(m.Share)}% meczów, {Percent(m.WinRate ?? 0)}% wygranych) — prawdopodobny pick{note}", preference);
		}

		if (solo is { IsAvoided: true } && m.Games <= SoloBanMaxTeamGames)
		{
			return new VetoPrediction(m.Map, "Ban", $"Rzadko grają ją drużynowo ({m.Games} z {totalGames} meczów), a {solo.AvoidingPlayers} z {solo.RatedPlayers} graczy unika jej także solo — prawdopodobny ban", preference);
		}

		return new VetoPrediction(m.Map, "Neutral", $"Grają ją okazjonalnie ({m.Games} meczów, {Percent(m.WinRate ?? 0)}% wygranych){note}", preference);
	}

	/// <summary>Too few team games: a call only from solo comfort (avoided → ban, top comfortable → pick), otherwise unknown.</summary>
	private static VetoPrediction FewTeamGames(MapMetrics m, int totalGames, MapComfort? solo, bool soloPick, double preference)
	{
		var prefix = $"Mało meczów drużynowych ({totalGames}) — wg meczów solo";

		if (solo is { IsAvoided: true } && m.Games <= RarelyPlayedGames)
		{
			return new VetoPrediction(m.Map, "Ban", $"{prefix}: {solo.AvoidingPlayers} z {solo.RatedPlayers} graczy jej unika — możliwy ban", preference);
		}

		if (solo is not null && soloPick)
		{
			return new VetoPrediction(m.Map, "Pick", $"{prefix}: {solo.RegularPlayers} z {solo.RatedPlayers} graczy gra ją regularnie ({Percent(solo.AvgWinRate ?? 0)}% wygranych) — możliwy pick", preference);
		}

		return new VetoPrediction(m.Map, "Unknown", $"Za mało meczów drużynowych ({totalGames}), by przewidzieć ich veto{SoloNote(solo)}", preference);
	}

	/// <summary>A short " · solo: …" suffix describing the comfort; empty without usable comfort.</summary>
	private static string SoloNote(MapComfort? solo) =>
		solo is null ? "" : $" · solo: {solo.RegularPlayers}/{solo.RatedPlayers} grają regularnie, {solo.AvoidingPlayers} unika";

	/// <summary>A fraction as a whole percentage.</summary>
	private static int Percent(double fraction) => (int)Math.Round(fraction * 100);

	#endregion
}
