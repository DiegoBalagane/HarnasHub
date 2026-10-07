#region Usings

using System.Globalization;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure rules turning a map's tendencies into Polish anti-strat suggestions. Each rule needs a minimum sample and
/// a clear majority before it speaks, and carries the confidence of the sample it is based on — a hint, not a verdict.</summary>
public static class AntiStratRules
{
	#region Public Fields

	/// <summary>Fewest rounds of a side a rule needs before it says anything.</summary>
	public const int MinRounds = 6;

	#endregion

	#region Public Methods

	/// <summary>All suggestions that apply to <paramref name="map"/>, T side first.</summary>
	public static List<AntiStratSuggestionDto> Evaluate(MapTendenciesDto map)
	{
		var suggestions = new List<AntiStratSuggestionDto>();
		TSide(map.T, suggestions);
		CtSide(map.Ct, suggestions);
		Players(map.Players, map.Rounds, suggestions);
		return suggestions;
	}

	/// <summary>Round time as shown in-game after freeze end, e.g. 70 → "1:10".</summary>
	public static string Clock(double seconds)
	{
		var total = (int)Math.Round(seconds);
		return $"{total / 60}:{total % 60:00}";
	}

	#endregion

	#region Private Methods

	private static void TSide(TSideTendenciesDto t, List<AntiStratSuggestionDto> output)
	{
		if (t.Rounds < MinRounds)
		{
			return;
		}

		var confidence = t.Confidence;
		var topTarget = t.Targets.FirstOrDefault(s => s.Label != nameof(MapArea.Mid));
		if (topTarget is { Percent: >= 60 })
		{
			var late = t.ExecTiming.FirstOrDefault(s => s.Label == "Late")?.Percent ?? 0;
			var when = t.AverageExecSecond is { } avg ? $", wejście średnio na {Clock(avg)}" : string.Empty;
			var action = late >= 50 && t.AverageExecSecond is { } a
				? $"stackujcie {topTarget.Label} albo wyjdźcie na agresję tuż przed {Clock(Math.Max(0, a - 10))}"
				: $"dajcie dodatkowego gracza na {topTarget.Label} i trzymajcie utility na ich wejście";
			output.Add(new("TTarget", "T", $"W {P(topTarget.Percent)} rund T idą na {topTarget.Label}{when} → {action}.",
				$"{topTarget.Count} z {t.Targets.Sum(s => s.Count)} rund z ustalonym celem", confidence));
		}

		var lateShare = t.ExecTiming.FirstOrDefault(s => s.Label == "Late");
		var fastShare = t.ExecTiming.FirstOrDefault(s => s.Label == "Fast");
		if (lateShare is { Percent: >= 50 })
		{
			output.Add(new("TSlow", "T", $"Grają wolno: {P(lateShare.Percent)} egzekucji po 1:15 → zbierajcie info i szukajcie picków na początku rundy, nie rotujcie za wcześnie.",
				$"{lateShare.Count} rund", confidence));
		}
		else if (fastShare is { Percent: >= 40 })
		{
			output.Add(new("TFast", "T", $"Grają szybko: {P(fastShare.Percent)} egzekucji przed 0:35 → utility od startu rundy, nie wychodźcie solo.",
				$"{fastShare.Count} rund", confidence));
		}

		foreach (var cluster in t.GrenadeClusters.Where(c => c.Type == "Smoke" && c.PerRoundPercent >= 40).Take(2))
		{
			var where = cluster.Area is null ? string.Empty : $" na {cluster.Area}";
			output.Add(new("TStandardSmoke", "T", $"Ten sam smoke{where} w {P(cluster.PerRoundPercent)} rund T → przygotujcie odpowiedź (np. molly/flash przez smoke) albo wyprzedźcie go.",
				$"{cluster.Rounds} z {t.Rounds} rund T", confidence));
		}

		var force = t.Pistol.AfterLostPistolBuys.FirstOrDefault(s => s.Label is "Force" or "SemiEco");
		var eco = t.Pistol.AfterLostPistolBuys.FirstOrDefault(s => s.Label == "Eco");
		if (t.Pistol.LostPistols >= 2 && force is { Percent: >= 50 })
		{
			output.Add(new("ForceAfterPistol", "T", $"Po przegranym pistolu zwykle force'ują ({P(force.Percent)}) → w 2. rundzie grajcie z dystansu, nie dawajcie im bliskich kątów.",
				$"{force.Count} z {t.Pistol.LostPistols} przegranych pistolówek", TendencyConfidence.For(t.Pistol.LostPistols * 4)));
		}
		else if (t.Pistol.LostPistols >= 2 && eco is { Percent: >= 60 })
		{
			output.Add(new("EcoAfterPistol", "T", $"Po przegranym pistolu ecują ({P(eco.Percent)}) → w 2. rundzie grajcie agresywniej i wychodźcie po info.",
				$"{eco.Count} z {t.Pistol.LostPistols} przegranych pistolówek", TendencyConfidence.For(t.Pistol.LostPistols * 4)));
		}
	}

	private static void CtSide(CtSideTendenciesDto ct, List<AntiStratSuggestionDto> output)
	{
		if (ct.Rounds < MinRounds)
		{
			return;
		}

		if (ct.Setups.FirstOrDefault() is { Percent: >= 40 } setup)
		{
			var weaker = CtSetupLabeler.WeakerSite(setup.Label);
			var action = weaker is { } site ? $"grajcie egzekucję na słabiej obstawione {MapAreaResolver.Label(site)}" : "szukajcie przewagi przez mida";
			output.Add(new("CtDefaultSetup", "CT", $"Ich standardowe ustawienie CT to {setup.Label} ({P(setup.Percent)} rund) → {action}.",
				$"{setup.Count} rund", ct.Confidence));
		}

		foreach (var stack in ct.Stacks.Where(s => s.Percent >= 30))
		{
			var other = stack.Label == nameof(MapArea.A) ? "B" : "A";
			output.Add(new("CtStack", "CT", $"W {P(stack.Percent)} rund stackują {stack.Label} → pokażcie się na {stack.Label} i szybko przejdźcie na {other}.",
				$"{stack.Count} rund", ct.Confidence));
		}

		if (ct.EarlyKillPercent >= 40)
		{
			output.Add(new("CtAggression", "CT", $"Agresywne CT: frag przed 0:25 w {P(ct.EarlyKillPercent)} rund → nie rushujcie, wejście na utility.",
				$"{ct.EarlyKillRounds} z {ct.Rounds} rund CT", ct.Confidence));
		}

		var awpTotal = ct.AwpAreas.Sum(a => a.Count);
		if (awpTotal >= 3 && ct.AwpAreas.FirstOrDefault() is { Percent: >= 50 } awp)
		{
			output.Add(new("CtAwpSpot", "CT", $"Ich AWP najwięcej fraguje na {awp.Label} ({P(awp.Percent)} killi) → zasmokujcie albo zaflashujcie ten kąt przed wejściem.",
				$"{awp.Count} z {awpTotal} killi z AWP", TendencyConfidence.For(awpTotal * 2)));
		}

		if (ct.PostPlantRounds >= 4)
		{
			var saves = TendencyConfidence.Percent(ct.Saves, ct.PostPlantRounds);
			var retakes = TendencyConfidence.Percent(ct.Retakes, ct.PostPlantRounds);
			if (saves >= 50)
			{
				output.Add(new("CtSaves", "CT", $"Po plancie często odpuszczają i save'ują ({P(saves)}) → grajcie na czas, nie szukajcie fragów na siłę.",
					$"{ct.Saves} z {ct.PostPlantRounds} rund po plancie", TendencyConfidence.For(ct.PostPlantRounds * 2)));
			}
			else if (retakes >= 70)
			{
				output.Add(new("CtRetakes", "CT", $"Prawie zawsze grają retake ({P(retakes)}) → po plancie zostawcie utility i grajcie crossfire'y na post-plancie.",
					$"{ct.Retakes} z {ct.PostPlantRounds} rund po plancie", TendencyConfidence.For(ct.PostPlantRounds * 2)));
			}
		}
	}

	private static void Players(List<PlayerTendencyDto> players, int rounds, List<AntiStratSuggestionDto> output)
	{
		var confidence = TendencyConfidence.For(rounds);
		if (players.FirstOrDefault(p => p.Role == "AWP") is { } awper)
		{
			output.Add(new("PlayerAwp", "Players", $"{awper.Name} to ich AWPer ({P(awper.AwpKillShare)} killi z AWP) → nie peekujcie jego kątów na sucho, najpierw flash/smoke.",
				$"{awper.AwpKills} killi z AWP", confidence));
		}

		if (players.FirstOrDefault(p => p.Role == "Entry") is { } entry)
		{
			output.Add(new("PlayerEntry", "Players", $"{entry.Name} wchodzi pierwszy w {P(entry.EntryRate)} rund → ustawcie crossfire na jego entry i gotowy trade.",
				entry.OpeningWinRate is { } win ? $"wygrywa {P(win)} otwierających pojedynków" : "brak otwierających pojedynków", confidence));
		}
	}

	private static string P(double percent) => $"{Math.Round(percent).ToString(CultureInfo.InvariantCulture)}%";

	#endregion
}
