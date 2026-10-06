#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Deterministic, rule-based insights from one match timeline: buy-type win rates, pistol rounds, opening duels
/// by side and zone, post-plant conversion / retakes and anti-eco losses. Every rule needs a minimum sample before it
/// says anything, and the result is capped at <see cref="MaxInsights"/>, problems first.</summary>
public static class MatchInsightRules
{
	#region Public Fields

	/// <summary>Upper bound of insights returned for one match.</summary>
	public const int MaxInsights = 6;

	/// <summary>Minimum rounds/duels a rule needs before it reports anything.</summary>
	public const int MinSample = 2;

	/// <summary>Minimum duels in one side+zone bucket before it can be called out.</summary>
	public const int MinZoneSample = 3;

	#endregion

	#region Public Methods

	/// <summary>Builds up to <see cref="MaxInsights"/> insights, negative first, then positive, then neutral.</summary>
	public static IReadOnlyList<MatchInsightDto> Build(MatchTimelineDto timeline)
	{
		if (!timeline.OurTeamResolved)
		{
			return [new MatchInsightDto("team-unknown", InsightTone.Neutral, "Nie rozpoznano naszej drużyny",
				"Żaden gracz z demki nie ma zapisanego SteamID64 w składzie, a wynik nie wskazał drużyny — uzupełnij SteamID64 w profilach graczy.", 0)];
		}

		var rounds = timeline.Rounds.Where(r => r.WeWon is not null && r.OurSide is not null).ToList();

		return BuyTypeInsights(rounds)
			.Concat(PistolInsights(rounds))
			.Concat(OpeningInsights(rounds))
			.Concat(BombInsights(rounds))
			.Concat(AntiEcoInsights(rounds))
			.OrderBy(i => i.Tone switch { InsightTone.Negative => 0, InsightTone.Positive => 1, _ => 2 })
			.ThenByDescending(i => i.SampleSize)
			.Take(MaxInsights)
			.ToList();
	}

	#endregion

	#region Private Methods

	private static IEnumerable<MatchInsightDto> BuyTypeInsights(List<MatchRoundDto> rounds)
	{
		foreach (var group in rounds.Where(r => r.OurEconomy is { BuyType: not BuyType.Pistol }).GroupBy(r => r.OurEconomy!.BuyType))
		{
			var total = group.Count();
			var won = group.Count(r => r.WeWon == true);
			var rate = (double)won / total;

			if (group.Key == BuyType.Full)
			{
				if (total >= 4 && rate < 0.5)
				{
					yield return new MatchInsightDto("buy-full", InsightTone.Negative, $"Pełne zakupy: tylko {won}/{total}",
						$"Na pełnym zakupie wygraliście {Percent(won, total)} rund — przy pełnym ekwipunku to za mało.", total);
				}

				continue;
			}

			if (total < MinSample)
			{
				continue;
			}

			var tone = won == 0 && total >= 3 ? InsightTone.Negative : rate >= 0.5 ? InsightTone.Positive : InsightTone.Neutral;
			yield return new MatchInsightDto($"buy-{group.Key.ToString().ToLowerInvariant()}", tone,
				$"{won}/{total} wygranych na {BuyName(group.Key)}",
				$"Rundy, w których kupiliście {BuyName(group.Key)}: wygrane {won} z {total} ({Percent(won, total)}).", total);
		}
	}

	private static IEnumerable<MatchInsightDto> PistolInsights(List<MatchRoundDto> rounds)
	{
		var pistols = rounds.Where(r => BuyTypeClassifier.IsPistolRound(r.Number)).ToList();
		if (pistols.Count == 0)
		{
			yield break;
		}

		var won = pistols.Count(r => r.WeWon == true);
		var details = pistols.Select(r =>
		{
			var followUps = rounds.Where(n => n.Number > r.Number && n.Number <= r.Number + 2).ToList();
			var converted = followUps.Count(n => n.WeWon == r.WeWon);
			var followUp = followUps.Count == 0 ? "" : r.WeWon == true
				? $", potem {converted}/{followUps.Count} wygranych"
				: $", potem {converted}/{followUps.Count} przegranych";
			return $"R{r.Number} ({r.OurSide}): {(r.WeWon == true ? "wygrana" : "przegrana")}{followUp}";
		});

		var tone = won == pistols.Count ? InsightTone.Positive : won == 0 ? InsightTone.Negative : InsightTone.Neutral;
		yield return new MatchInsightDto("pistols", tone, $"Pistolówki: {won}/{pistols.Count}", string.Join("; ", details) + ".", pistols.Count);
	}

	private static IEnumerable<MatchInsightDto> OpeningInsights(List<MatchRoundDto> rounds)
	{
		var openings = rounds
			.SelectMany(r => r.Kills.Where(k => k.IsOpening && k.ByUs is not null).Select(k => (Side: r.OurSide!.Value, Kill: k)))
			.ToList();

		var worstZone = openings
			.Select(o => (o.Side, Zone: o.Kill.ByUs == true ? o.Kill.KillerZone : o.Kill.VictimZone, Lost: o.Kill.ByUs == false))
			.Where(o => o.Zone is not null)
			.GroupBy(o => (o.Side, o.Zone))
			.Select(g => (g.Key.Side, g.Key.Zone, Total: g.Count(), Lost: g.Count(o => o.Lost)))
			.Where(g => g.Total >= MinZoneSample && (double)g.Lost / g.Total >= 0.6)
			.OrderByDescending(g => (double)g.Lost / g.Total)
			.ThenByDescending(g => g.Total)
			.FirstOrDefault();

		if (worstZone.Zone is not null)
		{
			yield return new MatchInsightDto("opening-zone", InsightTone.Negative,
				$"{Percent(worstZone.Lost, worstZone.Total)} otwarć przegranych na {worstZone.Side} {worstZone.Zone}",
				$"Pierwszy pojedynek rundy na {worstZone.Side} w strefie {worstZone.Zone}: przegrane {worstZone.Lost} z {worstZone.Total}.",
				worstZone.Total);
		}

		foreach (var side in openings.GroupBy(o => o.Side))
		{
			var total = side.Count();
			var won = side.Count(o => o.Kill.ByUs == true);
			var rate = (double)won / total;
			if (total < 4 || rate is >= 0.4 and <= 0.6)
			{
				continue;
			}

			yield return new MatchInsightDto($"opening-{side.Key.ToString().ToLowerInvariant()}",
				rate > 0.6 ? InsightTone.Positive : InsightTone.Negative,
				$"Otwarcia na {side.Key}: {won}/{total}",
				$"Pierwsze zabójstwo rundy na {side.Key} zdobyliście w {Percent(won, total)} rund.", total);
		}
	}

	private static IEnumerable<MatchInsightDto> BombInsights(List<MatchRoundDto> rounds)
	{
		var ourPlants = rounds.Where(r => r.OurSide == MapSide.T && r.Bomb is not null).ToList();
		if (ourPlants.Count >= MinSample)
		{
			var won = ourPlants.Count(r => r.WeWon == true);
			var rate = (double)won / ourPlants.Count;
			yield return new MatchInsightDto("post-plant", rate >= 0.7 ? InsightTone.Positive : rate < 0.5 ? InsightTone.Negative : InsightTone.Neutral,
				$"Po plancie: {won}/{ourPlants.Count}", $"Rundy z podłożoną przez Was bombą zamienione na wygraną: {Percent(won, ourPlants.Count)}.", ourPlants.Count);
		}

		var retakes = rounds.Where(r => r.OurSide == MapSide.CT && r.Bomb is not null).ToList();
		if (retakes.Count >= MinSample)
		{
			var won = retakes.Count(r => r.WeWon == true);
			var rate = (double)won / retakes.Count;
			yield return new MatchInsightDto("retakes", rate >= 0.5 ? InsightTone.Positive : rate < 0.25 ? InsightTone.Negative : InsightTone.Neutral,
				$"Retake'i: {won}/{retakes.Count}", $"Rundy na CT po plancie rywala, które wygraliście (rozbrojenie, eliminacja lub czas): {won} z {retakes.Count}.", retakes.Count);
		}
	}

	private static IEnumerable<MatchInsightDto> AntiEcoInsights(List<MatchRoundDto> rounds)
	{
		var antiEcos = rounds
			.Where(r => r.OpponentEconomy?.BuyType is BuyType.Eco or BuyType.SemiEco && r.OurEconomy?.BuyType is BuyType.Force or BuyType.Full)
			.ToList();
		if (antiEcos.Count < MinSample)
		{
			yield break;
		}

		var lost = antiEcos.Count(r => r.WeWon == false);
		var lostRounds = string.Join(", ", antiEcos.Where(r => r.WeWon == false).Select(r => $"R{r.Number}"));
		yield return lost > 0
			? new MatchInsightDto("anti-eco", InsightTone.Negative, $"Przegrane anty-eco: {lost}/{antiEcos.Count}",
				$"Rundy, w których rywal oszczędzał, a Wy mieliście zakup, przegrane: {lostRounds}.", antiEcos.Count)
			: new MatchInsightDto("anti-eco", InsightTone.Positive, $"Anty-eco: {antiEcos.Count}/{antiEcos.Count}",
				"Każda runda przeciwko oszczędzającemu rywalowi wygrana.", antiEcos.Count);
	}

	private static string BuyName(BuyType buyType) => buyType switch
	{
		BuyType.Eco => "eco",
		BuyType.SemiEco => "semi-eco",
		BuyType.Force => "force buyu",
		BuyType.Full => "pełnym zakupie",
		_ => "pistoletach"
	};

	private static string Percent(int part, int total) => $"{Math.Round(100.0 * part / Math.Max(total, 1))}%";

	#endregion
}
