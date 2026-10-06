using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>Everything the veto score of one map is built from; <paramref name="FaceitAdvantage"/> (−1…1, ours minus theirs from
/// the opponent report) is optional and weighted by the confidence of <paramref name="FaceitMinGames"/>.</summary>
public record MapVetoInput(
	MapName MapName,
	MapPoolStatus? Status,
	int Wins,
	int Losses,
	int Draws,
	int HeadToHeadWins,
	int HeadToHeadLosses,
	int OpponentPicks,
	int OpponentBans,
	int TacticCount,
	double? FaceitAdvantage = null,
	int FaceitMinGames = 0);

/// <summary>Deterministic, explainable veto scoring — deliberately simple additive points rather than a model, so every
/// number on screen has a sentence behind it.</summary>
public static class VetoScoring
{
	#region Public Fields

	/// <summary>How many maps our side bans in a standard BO1 veto of a 7-map pool.</summary>
	public const int OurBanCount = 3;

	/// <summary>How many maps are flagged as picks.</summary>
	public const int PickCount = 2;

	/// <summary>Cap on how many points the FACEIT advantage can add or take away, so it informs rather than overrides the pool.</summary>
	public const int MaxFaceitPoints = 20;

	#endregion

	#region Public Methods

	/// <summary>Scores every map, then orders them best first and labels the top as picks and the bottom (plus pool bans) as bans.</summary>
	public static List<MapVetoSuggestionDto> Suggest(IEnumerable<MapVetoInput> maps)
	{
		var scored = maps
			.Select(map => (Input: map, Result: Score(map)))
			.OrderByDescending(x => x.Result.Score)
			.ThenBy(x => x.Input.MapName.ToString())
			.ToList();

		var poolBans = scored.Count(x => x.Input.Status == MapPoolStatus.Ban);
		// Top the pool bans up to our ban count, but never ban so much that fewer than PickCount maps stay available.
		var extraBans = Math.Clamp(OurBanCount - poolBans, 0, Math.Max(0, scored.Count - poolBans - PickCount));
		var banned = scored
			.Where(x => x.Input.Status != MapPoolStatus.Ban)
			.TakeLast(extraBans)
			.Select(x => x.Input.MapName)
			.Concat(scored.Where(x => x.Input.Status == MapPoolStatus.Ban).Select(x => x.Input.MapName))
			.ToHashSet();
		var picked = scored
			.Where(x => !banned.Contains(x.Input.MapName) && x.Result.Score > 0)
			.Take(PickCount)
			.Select(x => x.Input.MapName)
			.ToHashSet();

		return scored
			.Select(x => new MapVetoSuggestionDto(
				x.Input.MapName.ToString(),
				x.Result.Score,
				banned.Contains(x.Input.MapName) ? "Ban" : picked.Contains(x.Input.MapName) ? "Pick" : "Neutral",
				x.Result.Reasons))
			.ToList();
	}

	#endregion

	#region Private Methods

	/// <summary>Adds up the points of one map and collects a reason for each non-zero contribution.</summary>
	private static (int Score, List<string> Reasons) Score(MapVetoInput map)
	{
		var reasons = new List<string>();
		var score = 0;

		switch (map.Status)
		{
			case MapPoolStatus.Ban:
				return (-100, ["Stały ban w puli map"]);
			case MapPoolStatus.Core:
				score += 30;
				reasons.Add("Pewniak w puli map");
				break;
			case MapPoolStatus.Playable:
				score += 10;
				reasons.Add("W puli map jako „gramy”");
				break;
			case MapPoolStatus.Learning:
				score -= 20;
				reasons.Add("Jeszcze w przygotowaniu");
				break;
		}

		var games = map.Wins + map.Losses + map.Draws;
		if (games == 0)
		{
			score -= 5;
			reasons.Add("Brak rozegranych meczów — niewiadoma");
		}
		else
		{
			// Laplace smoothing: one game can't swing the map to 100% or 0%.
			var smoothedWinRate = (map.Wins + 0.5 * map.Draws + 1) / (games + 2.0);
			score += (int)Math.Round((smoothedWinRate - 0.5) * 60);
			var winRate = (int)Math.Round(100.0 * map.Wins / games);
			reasons.Add($"Bilans ogólny {map.Wins}-{map.Losses}{(map.Draws > 0 ? $"-{map.Draws}" : "")} ({winRate}% wygranych)");
		}

		if (map.HeadToHeadWins + map.HeadToHeadLosses > 0)
		{
			score += Math.Clamp((map.HeadToHeadWins - map.HeadToHeadLosses) * 8, -24, 24);
			reasons.Add($"Z tym przeciwnikiem {map.HeadToHeadWins}-{map.HeadToHeadLosses}");
		}

		if (map.OpponentPicks > 0)
		{
			score -= Math.Min(map.OpponentPicks * 6, 18);
			reasons.Add($"Przeciwnik wybierał ją {map.OpponentPicks}× — ich mocna mapa");
		}

		if (map.OpponentBans > 0)
		{
			score += Math.Min(map.OpponentBans * 4, 12);
			reasons.Add($"Przeciwnik banował ją {map.OpponentBans}× — unikają jej");
		}

		if (map.TacticCount > 0)
		{
			score += Math.Min(map.TacticCount * 2, 10);
			reasons.Add($"{map.TacticCount} {TacticNoun(map.TacticCount)} w bibliotece");
		}

		if (map.FaceitAdvantage is { } advantage)
		{
			var confidence = MapAdvantage.Confidence(map.FaceitMinGames);
			var points = (int)Math.Clamp(Math.Round(advantage * 100 * MapAdvantage.VetoWeight(confidence)), -MaxFaceitPoints, MaxFaceitPoints);
			if (points != 0)
			{
				score += points;
				var percentagePoints = (int)Math.Round(advantage * 100);
				reasons.Add($"FACEIT: przewaga {(percentagePoints > 0 ? "+" : "")}{percentagePoints} pp nad przeciwnikiem (pewność {ConfidenceLabel(confidence)})");
			}
		}

		return (score, reasons);
	}

	/// <summary>Polish label of a confidence level, as shown in veto reasons.</summary>
	private static string ConfidenceLabel(ConfidenceLevel confidence) => confidence switch
	{
		ConfidenceLevel.High => "wysoka",
		ConfidenceLevel.Medium => "średnia",
		_ => "niska"
	};

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
