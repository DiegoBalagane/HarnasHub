using System.Globalization;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Everything the TL;DR rules look at: the finished map matrix, players to watch and form.</summary>
public record InsightInput(
	int TheirTeamGames,
	IReadOnlyList<MapComparisonDto> Maps,
	IReadOnlyList<MapPlayersToWatchDto> PlayersToWatch,
	OpponentFormDto Form);

/// <summary>Turns the report numbers into 3–5 plain-Polish TL;DR points. Each rule is a separate public method so it can be tested alone.</summary>
public static class OpponentInsightRules
{
	#region Public Fields

	/// <summary>Most points shown in the TL;DR.</summary>
	public const int MaxInsights = 5;

	/// <summary>Below this many team games every conclusion is flagged as rough.</summary>
	public const int LowSampleGames = 3;

	#endregion

	#region Public Methods

	/// <summary>All firing rules, most severe first, capped at <see cref="MaxInsights"/>.</summary>
	public static List<OpponentInsightDto> Build(InsightInput input) =>
		LowSample(input)
			.Concat(Opportunities(input))
			.Concat(Dangers(input))
			.Concat(MainMaps(input))
			.Concat(LikelyBans(input))
			.Concat(ThreatPlayer(input))
			.Concat(Form(input))
			.Concat(RosterChanges(input))
			.OrderBy(i => SeverityRank(i.Severity))
			.Take(MaxInsights)
			.ToList();

	/// <summary>Warns when there are too few team games for solid conclusions.</summary>
	public static IEnumerable<OpponentInsightDto> LowSample(InsightInput input)
	{
		if (input.TheirTeamGames < LowSampleGames)
		{
			yield return new("LowSample", "Warning", $"Mało meczów drużynowych rywala ({input.TheirTeamGames}) — wnioski są orientacyjne", $"{input.TheirTeamGames} meczów w oknie");
		}
	}

	/// <summary>Their one or two most played maps when together they make up at least half of their games.</summary>
	public static IEnumerable<OpponentInsightDto> MainMaps(InsightInput input)
	{
		if (input.TheirTeamGames < LowSampleGames)
		{
			yield break;
		}

		var top = input.Maps.Where(m => m.TheirGames > 0).OrderByDescending(m => m.TheirGames).Take(2).ToList();
		var share = top.Sum(m => m.TheirShare);
		if (top.Count > 0 && share >= 50)
		{
			var names = string.Join(" i ", top.Select(m => m.MapName));
			yield return new("MainMaps", "Info", $"Grają głównie {names} ({Pct(share)}% meczów)", string.Join(", ", top.Select(m => $"{m.MapName}: {m.TheirGames}")));
		}
	}

	/// <summary>Maps they (almost) never play — near-certain bans.</summary>
	public static IEnumerable<OpponentInsightDto> LikelyBans(InsightInput input)
	{
		var bans = input.Maps.Where(m => m.Prediction == "Ban").Take(2).ToList();
		if (bans.Count > 0)
		{
			var names = string.Join(" i ", bans.Select(m => m.MapName));
			var text = bans.Count == 1 ? $"Nie grają {names} — prawie pewny ban" : $"Nie grają {names} — prawie pewne bany";
			yield return new("LikelyBan", "Info", text, string.Join(", ", bans.Select(m => $"{m.MapName}: {m.TheirGames} z {input.TheirTeamGames}")));
		}
	}

	/// <summary>Maps where they are weak and we are clearly better — pick candidates.</summary>
	public static IEnumerable<OpponentInsightDto> Opportunities(InsightInput input) =>
		input.Maps
			.Where(m => m.TheirGames >= 3 && m.TheirWinRate <= 45 && m.OurWinRate >= 55 && m.Advantage >= 10)
			.OrderByDescending(m => m.Advantage)
			.Take(2)
			.Select(m => new OpponentInsightDto(
				"Opportunity",
				"High",
				$"Słabi na {m.MapName} ({Pct(m.TheirWinRate!.Value)}%), my {Pct(m.OurWinRate!.Value)}% → pick",
				$"oni {m.TheirWins}/{m.TheirGames}, my {Num(m.OurWins)}/{m.OurGames}, przewaga {Signed(m.Advantage)} pp"));

	/// <summary>Maps where they hold a real edge over us — ban candidates.</summary>
	public static IEnumerable<OpponentInsightDto> Dangers(InsightInput input) =>
		input.Maps
			.Where(m => m.Advantage <= -10 && m.Confidence != nameof(ConfidenceLevel.Low) && m.TheirWinRate.HasValue)
			.OrderBy(m => m.Advantage)
			.Take(2)
			.Select(m => new OpponentInsightDto(
				"Danger",
				"Warning",
				$"Uwaga na {m.MapName}: oni {Pct(m.TheirWinRate!.Value)}%, my {(m.OurWinRate.HasValue ? $"{Pct(m.OurWinRate.Value)}%" : "brak meczów")} — kandydat do bana",
				$"przewaga {Signed(m.Advantage)} pp, pewność {ConfidenceLabel(m.Confidence)}"));

	/// <summary>The single most dangerous player (by ADR, then K/D) with at least 3 games on a map.</summary>
	public static IEnumerable<OpponentInsightDto> ThreatPlayer(InsightInput input)
	{
		var best = input.PlayersToWatch
			.SelectMany(map => map.Players.Where(p => p.Games >= 3).Select(p => (Map: map.MapName, Player: p)))
			.OrderByDescending(x => x.Player.Adr ?? 0)
			.ThenByDescending(x => x.Player.KdRatio)
			.FirstOrDefault();

		if (best.Player is null)
		{
			yield break;
		}

		var stat = best.Player.Adr is { } adr ? $"ADR {Pct(adr)}" : $"K/D {Num(best.Player.KdRatio)}";
		yield return new("Player", "Warning", $"Groźny: {best.Player.Nickname} ({stat} na {best.Map})", $"{best.Player.Games} meczów, K/D {Num(best.Player.KdRatio)}");
	}

	/// <summary>A hot or cold run over their last five team games.</summary>
	public static IEnumerable<OpponentInsightDto> Form(InsightInput input)
	{
		var recent = input.Form.LastGames.Take(5).ToList();
		if (recent.Count < 5)
		{
			yield break;
		}

		var wins = recent.Count(g => g.Won);
		if (wins >= 4)
		{
			yield return new("Form", "Warning", $"W formie: {wins} z 5 ostatnich meczów wygranych", $"seria {input.Form.Streak}");
		}
		else if (wins <= 1)
		{
			yield return new("Form", "Info", $"Słaba forma: tylko {wins} z 5 ostatnich meczów wygranych", $"seria {input.Form.Streak}");
		}
	}

	/// <summary>New faces in their latest lineups.</summary>
	public static IEnumerable<OpponentInsightDto> RosterChanges(InsightInput input)
	{
		if (input.Form.NewPlayers.Count > 0)
		{
			yield return new("Roster", "Info", $"Zmiany w składzie: {string.Join(", ", input.Form.NewPlayers)} w ostatnich meczach", "gracze nieobecni we wcześniejszych meczach");
		}
	}

	#endregion

	#region Private Methods

	/// <summary>Sort key: High before Warning before Info.</summary>
	private static int SeverityRank(string severity) => severity switch
	{
		"High" => 0,
		"Warning" => 1,
		_ => 2
	};

	/// <summary>Polish label of a confidence level name.</summary>
	private static string ConfidenceLabel(string confidence) => confidence switch
	{
		nameof(ConfidenceLevel.High) => "wysoka",
		nameof(ConfidenceLevel.Medium) => "średnia",
		_ => "niska"
	};

	/// <summary>A percentage rounded to a whole number.</summary>
	private static string Pct(double value) => Math.Round(value).ToString("0", CultureInfo.InvariantCulture);

	/// <summary>A number with up to two decimals, invariant culture.</summary>
	private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

	/// <summary>Percentage points with an explicit sign.</summary>
	private static string Signed(double value) => (value > 0 ? "+" : "") + Pct(value);

	#endregion
}
