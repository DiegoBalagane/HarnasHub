using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The opponent's expected stance on a map: <paramref name="Prediction"/> is "Ban", "Pick", "Neutral" or "Unknown";
/// <paramref name="Preference"/> ranks maps for the veto simulation (higher = they like it more, 0 = never played).</summary>
public record VetoPrediction(MapName Map, string Prediction, string Reason, double Preference);

/// <summary>Guesses the opponent's veto from what they play — FACEIT doesn't expose ban order, only the map that was played.
/// Trust order: recent team games (recency-weighted) › the active lineup's recent solo games (<see cref="IndividualSignal"/>
/// shrinkage) › their lifetime numbers (<see cref="LifetimeMapCalculator"/>, lowest weight). "They don't play X → ban" needs
/// <see cref="SampleThresholds.MinTeamGamesForAvoidance"/> team games and must not contradict the individual data.</summary>
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
	/// optionally the active lineup's solo <paramref name="comfort"/> and <paramref name="lifetime"/> numbers per map.</summary>
	public static Dictionary<MapName, VetoPrediction> Predict(
		IReadOnlyDictionary<MapName, MapMetrics> theirs,
		int totalGames,
		IReadOnlyDictionary<MapName, MapComfort>? comfort = null,
		IReadOnlyDictionary<MapName, MapLifetime>? lifetime = null)
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
			var life = lifetime?.GetValueOrDefault(m.Map);
			var evidence = Math.Max(m.Games, theirs.Count == 0 ? 0 : (double)totalGames / theirs.Count);
			var preference = LifetimeMapCalculator.Blend(
				IndividualSignal.BlendPreference(Preference(m), m.Games, totalGames, theirs.Count, solo),
				evidence + (solo is null ? 0 : IndividualSignal.PriorGames),
				life);
			var context = new MapContext(m, totalGames, solo, life, preference);

			return totalGames < MinGamesForPrediction
				? FewTeamGames(context, soloPicks.Contains(m.Map))
				: FromTeamGames(context, likelyPicks.Contains(m.Map));
		});
	}

	/// <summary>How much they like a map from team games: recency-weighted share plus smoothed win rate; 0 for a map they never play.</summary>
	public static double Preference(MapMetrics metrics) =>
		metrics.Games == 0 ? 0 : metrics.DecisionShare + metrics.SmoothedWinRate;

	/// <summary>Whether the individual data says they do play the map — regulars solo or a lot of lifetime matches — so "they
	/// don't play it" from team games alone would be a contradiction.</summary>
	public static bool PlayedIndividually(MapComfort? solo, MapLifetime? lifetime) =>
		(solo is not null && !solo.IsAvoided && solo.RegularPlayers >= 2) || lifetime is { IsExperienced: true };

	#endregion

	#region Private Methods

	/// <summary>Everything one map's prediction is built from.</summary>
	private sealed record MapContext(MapMetrics Metrics, int TotalGames, MapComfort? Solo, MapLifetime? Lifetime, double Preference);

	/// <summary>Enough team games for picks; "they don't play it" still needs the larger sample and no individual contradiction.</summary>
	private static VetoPrediction FromTeamGames(MapContext c, bool likelyPick)
	{
		var (m, total, solo, life) = (c.Metrics, c.TotalGames, c.Solo, c.Lifetime);
		var note = SoloNote(solo) + LifetimeNote(life);

		if (m.Games <= RarelyPlayedGames)
		{
			if (PlayedIndividually(solo, life))
			{
				return new VetoPrediction(m.Map, "Unknown", $"Rzadko grają drużynowo ({m.Games} z {total}), ale indywidualnie dużo ({IndividualEvidence(solo, life)}) → niepewne", c.Preference);
			}

			if (total >= SampleThresholds.MinTeamGamesForAvoidance)
			{
				return new VetoPrediction(m.Map, "Ban", $"Prawie jej nie grają ({m.Games} z {total} meczów) — prawdopodobny ban{note}", c.Preference);
			}

			if (solo is { IsAvoided: true })
			{
				return new VetoPrediction(m.Map, "Ban", $"Rzadko grają ją drużynowo ({m.Games} z {total} meczów), a {solo.AvoidingPlayers} z {solo.RatedPlayers} graczy unika jej także solo — możliwy ban", c.Preference);
			}

			return new VetoPrediction(m.Map, "Unknown", $"Rzadko grają drużynowo ({m.Games} z {total}) — za mało meczów (< {SampleThresholds.MinTeamGamesForAvoidance}), by mówić o banie{note}", c.Preference);
		}

		if (likelyPick)
		{
			return new VetoPrediction(m.Map, "Pick", $"Jedna z ich ulubionych map ({Percent(m.Share)}% meczów{WinRateNote(m)}) — prawdopodobny pick{note}", c.Preference);
		}

		if (solo is { IsAvoided: true } && m.Games <= SoloBanMaxTeamGames && life is not { IsExperienced: true })
		{
			return new VetoPrediction(m.Map, "Ban", $"Rzadko grają ją drużynowo ({m.Games} z {total} meczów), a {solo.AvoidingPlayers} z {solo.RatedPlayers} graczy unika jej także solo — prawdopodobny ban", c.Preference);
		}

		return new VetoPrediction(m.Map, "Neutral", $"Grają ją okazjonalnie ({m.Games} {VetoNotes.MatchNoun(m.Games)}{WinRateNote(m)}){note}", c.Preference);
	}

	/// <summary>Too few team games: a call only from solo comfort (avoided → ban, top comfortable → pick), otherwise unknown.</summary>
	private static VetoPrediction FewTeamGames(MapContext c, bool soloPick)
	{
		var (m, total, solo, life) = (c.Metrics, c.TotalGames, c.Solo, c.Lifetime);
		var prefix = $"Mało meczów drużynowych ({total}) — wg meczów solo";

		if (solo is { IsAvoided: true } && m.Games <= RarelyPlayedGames && life is not { IsExperienced: true })
		{
			return new VetoPrediction(m.Map, "Ban", $"{prefix}: {solo.AvoidingPlayers} z {solo.RatedPlayers} graczy jej unika — możliwy ban", c.Preference);
		}

		if (solo is not null && soloPick)
		{
			return new VetoPrediction(m.Map, "Pick", $"{prefix}: {solo.RegularPlayers} z {solo.RatedPlayers} graczy gra ją regularnie ({Percent(solo.AvgWinRate ?? 0)}% wygranych) — możliwy pick", c.Preference);
		}

		return new VetoPrediction(m.Map, "Unknown", $"Za mało meczów drużynowych ({total}), by przewidzieć ich veto{SoloNote(solo)}{LifetimeNote(life)}", c.Preference);
	}

	/// <summary>", 58% wygranych" from <see cref="SampleThresholds.MinGamesForWinRate"/> games on the map, otherwise nothing.</summary>
	private static string WinRateNote(MapMetrics m) =>
		SampleThresholds.HasWinRateSample(m.Games) ? $", {Percent(m.WinRate ?? 0)}% wygranych" : "";

	/// <summary>What the individual data says, for the "niepewne" reason.</summary>
	private static string IndividualEvidence(MapComfort? solo, MapLifetime? life)
	{
		var parts = new List<string>();
		if (life is { IsExperienced: true })
		{
			parts.Add($"{life.Matches} meczów lifetime składu");
		}

		if (solo is not null && !solo.IsAvoided && solo.RegularPlayers >= 2)
		{
			parts.Add($"{solo.RegularPlayers} z {solo.RatedPlayers} graczy gra ją regularnie solo");
		}

		return string.Join(", ", parts);
	}

	/// <summary>A short " · solo: …" suffix describing the comfort; empty without usable comfort.</summary>
	private static string SoloNote(MapComfort? solo) =>
		solo is null ? "" : $" · solo: {solo.RegularPlayers}/{solo.RatedPlayers} grają regularnie, {solo.AvoidingPlayers} unika";

	/// <summary>A short " · lifetime: …" suffix; empty without lifetime matches.</summary>
	private static string LifetimeNote(MapLifetime? life) =>
		life is not { Matches: > 0 } ? "" : $" · lifetime: {life.Matches} meczów składu";

	/// <summary>A fraction as a whole percentage.</summary>
	private static int Percent(double fraction) => (int)Math.Round(fraction * 100);

	#endregion
}
