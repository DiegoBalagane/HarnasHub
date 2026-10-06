using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>The score of one map: total points, one Polish reason per signal, a short <paramref name="Note"/> naming the real
/// drivers (used in the simulated veto) and whether the map may fill a ban slot — only with negative evidence beyond solo form.</summary>
public record VetoMapScore(int Score, List<string> Reasons, string Note, bool BanCandidate);

/// <summary>Additive points per map. Order of trust: the coach's pool status › our record and the opponent's per-map strength (both
/// only from <see cref="SampleThresholds.MinGamesForWinRate"/> games) › head-to-head, recorded vetoes, tactics › our solo form (±5).</summary>
public static class VetoMapScorer
{
	#region Public Fields

	/// <summary>Points for a comfort ("Core") map.</summary>
	public const int CorePoints = 40;

	/// <summary>Points for a "Playable" map.</summary>
	public const int PlayablePoints = 10;

	/// <summary>Points for a map still being learned.</summary>
	public const int LearningPoints = -25;

	/// <summary>Cap of our record's contribution (smoothed win rate − 50%, × 60).</summary>
	public const int MaxRecordPoints = 25;

	/// <summary>Cap of the opponent-strength contribution (50% − their weighted smoothed win rate, × 60).</summary>
	public const int MaxOpponentPoints = 15;

	/// <summary>Multiplier of our solo prior's distance from 50% — with the prior capped to 0.4–0.6 this is at most ±5 points.</summary>
	public const double SoloWeight = 50;

	#endregion

	#region Public Methods

	/// <summary>Scores one map.</summary>
	public static VetoMapScore Score(MapVetoInput map)
	{
		if (map.Status == MapPoolStatus.Ban)
		{
			return new VetoMapScore(-100, ["Stały ban w puli map"], "w puli: stały ban", true);
		}

		var parts = new List<Part>();
		var reasons = new List<string>();
		Pool(map.Status, parts, reasons);
		var lowSample = Record(map, parts, reasons);
		Opponent(map, parts, reasons);
		History(map, parts, reasons);

		var solo = map.OurSoloPrior is { } prior ? (int)Math.Round((prior - 0.5) * SoloWeight) : 0;
		if (solo != 0)
		{
			parts.Add(new Part(solo, "", Solo: true));
			reasons.Add($"Forma indywidualna naszych graczy na tej mapie (solo ~{Math.Round(map.OurSoloPrior!.Value * 100):0}%) — składnik pomocniczy");
		}

		var notes = parts
			.Where(p => !p.Solo && p.Points != 0)
			.OrderByDescending(p => Math.Abs(p.Points))
			.Take(2)
			.Select(p => p.Note)
			.ToList();
		if (lowSample is not null)
		{
			notes.Add(lowSample);
		}

		return new VetoMapScore(
			parts.Sum(p => p.Points),
			reasons,
			notes.Count == 0 ? "brak wyraźnych sygnałów" : string.Join(", ", notes),
			parts.Where(p => !p.Solo).Sum(p => p.Points) < 0);
	}

	#endregion

	#region Private Methods

	/// <summary>One signal's points and its short note; solo parts never decide a ban and are left out of the note.</summary>
	private sealed record Part(int Points, string Note, bool Solo = false);

	/// <summary>The coach's pool status — the primary signal.</summary>
	private static void Pool(MapPoolStatus? status, List<Part> parts, List<string> reasons)
	{
		switch (status)
		{
			case MapPoolStatus.Core:
				parts.Add(new Part(CorePoints, "mapa komfortowa w puli"));
				reasons.Add("Mapa komfortowa w puli (pewniak)");
				break;
			case MapPoolStatus.Playable:
				parts.Add(new Part(PlayablePoints, "w puli: gramy"));
				reasons.Add("W puli map jako „gramy”");
				break;
			case MapPoolStatus.Learning:
				parts.Add(new Part(LearningPoints, "w puli: w przygotowaniu"));
				reasons.Add("W puli: jeszcze w przygotowaniu");
				break;
			default:
				reasons.Add("Brak statusu w puli map");
				break;
		}
	}

	/// <summary>Our record (internal + FACEIT team games), only from the minimum sample; returns the "za mało danych" note, if any.</summary>
	private static string? Record(MapVetoInput map, List<Part> parts, List<string> reasons)
	{
		var games = map.Wins + map.Losses + map.Draws + map.OurFaceitGames;
		var wins = map.Wins + map.OurFaceitWins;
		var losses = map.Losses + map.OurFaceitGames - map.OurFaceitWins;
		var record = $"{wins}-{losses}{(map.Draws > 0 ? $"-{map.Draws}" : "")}";
		var noun = VetoNotes.MatchNoun(games);

		if (games == 0)
		{
			reasons.Add("Brak naszych meczów na tej mapie");
			return "my: brak meczów";
		}

		if (!SampleThresholds.HasWinRateSample(games))
		{
			reasons.Add($"Nasz bilans {record} — za mało danych ({games} {noun}), nie wpływa na rekomendację");
			return $"za mało danych (my: {games} {noun})";
		}

		var effectiveWins = wins + 0.5 * map.Draws;
		var winRate = (int)Math.Round(100.0 * effectiveWins / games);
		var smoothed = MapAdvantage.SmoothedWinRate(effectiveWins, games);
		var points = (int)Math.Clamp(Math.Round((smoothed - 0.5) * 60), -MaxRecordPoints, MaxRecordPoints);
		var source = map.OurFaceitGames > 0 ? $", w tym {map.OurFaceitGames} na FACEIT" : "";
		parts.Add(new Part(points, $"nasz bilans {record} ({winRate}%)"));
		reasons.Add($"Nasz bilans {record} ({winRate}% wygranych w {games} meczach{source})");
		return null;
	}

	/// <summary>The opponent's per-map strength from their team games (recency-weighted), only from the minimum sample.</summary>
	private static void Opponent(MapVetoInput map, List<Part> parts, List<string> reasons)
	{
		if (map.TheirGames == 0 || map.TheirWinRate is not { } weighted)
		{
			return;
		}

		var noun = VetoNotes.MatchNoun(map.TheirGames);
		if (!SampleThresholds.HasWinRateSample(map.TheirGames))
		{
			reasons.Add($"Rywal: {map.TheirGames} {noun} na tej mapie — za mało, by oceniać ich skuteczność");
			return;
		}

		var points = (int)Math.Clamp(Math.Round((0.5 - weighted) * 60), -MaxOpponentPoints, MaxOpponentPoints);
		if (points == 0)
		{
			return;
		}

		var raw = (int)Math.Round(100.0 * map.TheirWins / map.TheirGames);
		var label = points < 0 ? "przewaga rywala" : "słabość rywala";
		parts.Add(new Part(points, $"{label}: ich {raw}% przy {map.TheirGames} meczach"));
		reasons.Add($"{char.ToUpperInvariant(label[0])}{label[1..]}: ich {raw}% wygranych przy {map.TheirGames} meczach");
	}

	/// <summary>Head-to-head record, the opponent's recorded picks/bans and our tactic count.</summary>
	private static void History(MapVetoInput map, List<Part> parts, List<string> reasons)
	{
		if (map.HeadToHeadWins + map.HeadToHeadLosses > 0)
		{
			parts.Add(new Part(Math.Clamp((map.HeadToHeadWins - map.HeadToHeadLosses) * 8, -24, 24), $"z nimi {map.HeadToHeadWins}-{map.HeadToHeadLosses}"));
			reasons.Add($"Z tym przeciwnikiem {map.HeadToHeadWins}-{map.HeadToHeadLosses}");
		}

		if (map.OpponentPicks > 0)
		{
			parts.Add(new Part(-Math.Min(map.OpponentPicks * 6, 18), $"rywal wybierał ją {map.OpponentPicks}×"));
			reasons.Add($"Przeciwnik wybierał ją {map.OpponentPicks}× — ich mocna mapa");
		}

		if (map.OpponentBans > 0)
		{
			parts.Add(new Part(Math.Min(map.OpponentBans * 4, 12), $"rywal ją banował {map.OpponentBans}×"));
			reasons.Add($"Przeciwnik banował ją {map.OpponentBans}× — unikają jej");
		}

		if (map.TacticCount > 0)
		{
			var noun = TacticNoun(map.TacticCount);
			parts.Add(new Part(Math.Min(map.TacticCount * 2, 10), $"{map.TacticCount} {noun} w bibliotece"));
			reasons.Add($"{map.TacticCount} {noun} w bibliotece");
		}
	}

	/// <summary>Polish plural of "taktyka": 1 taktyka, 2–4 taktyki (except 12–14), otherwise taktyk.</summary>
	private static string TacticNoun(int count)
	{
		if (count == 1)
		{
			return "taktyka";
		}

		var lastDigit = count % 10;
		var lastTwo = count % 100;
		return lastDigit is >= 2 and <= 4 && lastTwo is < 12 or > 14 ? "taktyki" : "taktyk";
	}

	#endregion
}
