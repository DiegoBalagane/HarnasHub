namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Deterministic Polish insights over a map's multi-match aggregates: side imbalance, pistols, buy types, site
/// preference, retakes, weakest/strongest opening zone, trades and clutchers. Each rule needs a minimum sample.</summary>
public static class MapInsightRules
{
	#region Public Fields

	/// <summary>Upper bound of insights returned.</summary>
	public const int MaxInsights = 8;

	/// <summary>Minimum rounds on a side/buy type before it is judged.</summary>
	public const int MinRounds = 6;

	/// <summary>Minimum events (duels, plants, retakes, pistols) before they are judged.</summary>
	public const int MinEvents = 4;

	#endregion

	#region Public Methods

	/// <summary>Builds up to <see cref="MaxInsights"/> insights, negative first, then positive, then neutral.</summary>
	public static IReadOnlyList<MatchInsightDto> Build(MapAnalyticsDto data)
	{
		if (data.MatchesAnalyzed == 0)
		{
			return [];
		}

		return SideInsights(data)
			.Concat(PistolInsights(data))
			.Concat(BuyInsights(data))
			.Concat(SiteInsights(data))
			.Concat(RetakeInsights(data))
			.Concat(OpeningInsights(data))
			.Concat(TradeInsights(data))
			.Concat(ClutchInsights(data))
			.OrderBy(i => i.Tone switch { InsightTone.Negative => 0, InsightTone.Positive => 1, _ => 2 })
			.ThenByDescending(i => i.SampleSize)
			.Take(MaxInsights)
			.ToList();
	}

	#endregion

	#region Private Methods

	private static IEnumerable<MatchInsightDto> SideInsights(MapAnalyticsDto data)
	{
		if (data.TSide.Total < MinRounds || data.CtSide.Total < MinRounds)
		{
			yield break;
		}

		var t = Ratio(data.TSide);
		var ct = Ratio(data.CtSide);
		if (Math.Abs(t - ct) < 0.15)
		{
			yield break;
		}

		var weaker = t < ct ? "T" : "CT";
		yield return new MatchInsightDto("side-imbalance", InsightTone.Negative, $"Słabsza strona: {weaker}",
			$"Na tej mapie wygrywacie {Percent(data.TSide)} rund na T i {Percent(data.CtSide)} na CT.", data.TSide.Total + data.CtSide.Total);
	}

	private static IEnumerable<MatchInsightDto> PistolInsights(MapAnalyticsDto data)
	{
		if (data.Pistol.Total < MinEvents)
		{
			yield break;
		}

		var rate = Ratio(data.Pistol);
		if (rate <= 0.4 || rate >= 0.6)
		{
			yield return new MatchInsightDto("pistols", rate >= 0.6 ? InsightTone.Positive : InsightTone.Negative,
				$"Pistolówki: {data.Pistol.Won}/{data.Pistol.Total}", $"Skuteczność w pistolówkach na tej mapie: {Percent(data.Pistol)}.", data.Pistol.Total);
		}
	}

	private static IEnumerable<MatchInsightDto> BuyInsights(MapAnalyticsDto data)
	{
		foreach (var buy in data.BuyTypes.Where(b => b.BuyType is "Force" or "Full" && b.Total >= MinRounds))
		{
			var rate = (double)buy.Won / buy.Total;
			if (buy.BuyType == "Force" && rate <= 0.25)
			{
				yield return new MatchInsightDto("buy-force", InsightTone.Negative, $"Force buye: {buy.Won}/{buy.Total}",
					$"Force buye na tej mapie rzadko się opłacają ({Percent(buy.Won, buy.Total)} wygranych).", buy.Total);
			}
			else if (buy.BuyType == "Full" && rate < 0.5)
			{
				yield return new MatchInsightDto("buy-full", InsightTone.Negative, $"Pełne zakupy: {buy.Won}/{buy.Total}",
					$"Na pełnym zakupie wygrywacie tylko {Percent(buy.Won, buy.Total)} rund.", buy.Total);
			}
			else if (buy.BuyType == "Full" && rate >= 0.65)
			{
				yield return new MatchInsightDto("buy-full", InsightTone.Positive, $"Pełne zakupy: {buy.Won}/{buy.Total}",
					$"Na pełnym zakupie wygrywacie {Percent(buy.Won, buy.Total)} rund.", buy.Total);
			}
		}
	}

	private static IEnumerable<MatchInsightDto> SiteInsights(MapAnalyticsDto data)
	{
		var known = data.TSites.Where(s => s.Site is not null).ToList();
		var total = known.Sum(s => s.Total);
		if (total < MinEvents + 2)
		{
			yield break;
		}

		var top = known.OrderByDescending(s => s.Total).First();
		if ((double)top.Total / total >= 0.65)
		{
			yield return new MatchInsightDto("site-preference", InsightTone.Neutral, $"Wejścia głównie na {top.Site}",
				$"{Percent(top.Total, total)} podłożeń bomby na T to site {top.Site} (skuteczność {Percent(top.Won, top.Total)}) — łatwo to przewidzieć.", total);
		}
	}

	private static IEnumerable<MatchInsightDto> RetakeInsights(MapAnalyticsDto data)
	{
		var attempts = data.CtRetakes.Sum(s => s.Total);
		var won = data.CtRetakes.Sum(s => s.Won);
		if (attempts < MinEvents)
		{
			yield break;
		}

		var rate = (double)won / attempts;
		if (rate <= 0.3 || rate >= 0.5)
		{
			yield return new MatchInsightDto("retake", rate >= 0.5 ? InsightTone.Positive : InsightTone.Negative,
				$"Retake: {won}/{attempts}", $"Rundy na CT po podłożeniu bomby przez rywala wygrane: {Percent(won, attempts)}.", attempts);
		}
	}

	private static IEnumerable<MatchInsightDto> OpeningInsights(MapAnalyticsDto data)
	{
		var zones = data.Openings.Where(o => o.Zone is not null && o.Won + o.Lost >= MinEvents).ToList();
		var worst = zones.OrderBy(o => (double)o.Won / (o.Won + o.Lost)).FirstOrDefault();
		if (worst is not null && (double)worst.Won / (worst.Won + worst.Lost) <= 0.35)
		{
			yield return new MatchInsightDto("opening-worst", InsightTone.Negative, $"Przegrane otwarcia: {worst.Side} {worst.Zone}",
				$"Otwierające pojedynki na {worst.Side} w strefie {worst.Zone}: wygrane tylko {Percent(worst.Won, worst.Won + worst.Lost)}.", worst.Won + worst.Lost);
		}

		var best = zones.OrderByDescending(o => (double)o.Won / (o.Won + o.Lost)).FirstOrDefault();
		if (best is not null && (double)best.Won / (best.Won + best.Lost) >= 0.65)
		{
			yield return new MatchInsightDto("opening-best", InsightTone.Positive, $"Mocne otwarcia: {best.Side} {best.Zone}",
				$"Otwierające pojedynki na {best.Side} w strefie {best.Zone}: wygrane {Percent(best.Won, best.Won + best.Lost)}.", best.Won + best.Lost);
		}
	}

	private static IEnumerable<MatchInsightDto> TradeInsights(MapAnalyticsDto data)
	{
		if (data.Trades.OurDeaths < 10)
		{
			yield break;
		}

		var rate = (double)data.Trades.OurTradedDeaths / data.Trades.OurDeaths;
		if (rate <= 0.4 || rate >= 0.6)
		{
			yield return new MatchInsightDto("trades", rate >= 0.6 ? InsightTone.Positive : InsightTone.Negative,
				$"Trade'y: {Percent(data.Trades.OurTradedDeaths, data.Trades.OurDeaths)}",
				$"{data.Trades.OurTradedDeaths} z {data.Trades.OurDeaths} Waszych śmierci zostało odpłaconych w 5 s.", data.Trades.OurDeaths);
		}
	}

	private static IEnumerable<MatchInsightDto> ClutchInsights(MapAnalyticsDto data)
	{
		var top = data.TopClutchers.FirstOrDefault(c => c.Won >= 2);
		if (top is not null)
		{
			yield return new MatchInsightDto("clutcher", InsightTone.Positive, $"Clutchmaster: {top.Name}",
				$"{top.Name} wygrał {top.Won} z {top.Attempts} clutchy na tej mapie.", top.Attempts);
		}
	}

	private static double Ratio(WinRateDto rate) => rate.Total == 0 ? 0 : (double)rate.Won / rate.Total;

	private static string Percent(WinRateDto rate) => Percent(rate.Won, rate.Total);

	private static string Percent(int part, int total) => $"{Math.Round(100.0 * part / Math.Max(total, 1))}%";

	#endregion
}
