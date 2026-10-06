using System.Globalization;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>TL;DR rules built on individual form (solo comfort and player form arrows); each rule is public so it can be tested
/// alone, and <see cref="OpponentInsightRules.Build"/> ranks them together with the team rules.</summary>
public static class IndividualInsightRules
{
	#region Public Fields

	/// <summary>Our player needs this many solo games on a map before it is called a strong map.</summary>
	public const int StrongMapMinSoloGames = 5;

	/// <summary>Solo K/D from which a map counts as strong for our player.</summary>
	public const double StrongMapMinKd = 1.2;

	/// <summary>Average solo win rate (percent) of the regulars from which a comfortable map is flagged as a possible pick.</summary>
	public const double ComfortPickMinWinRate = 55;

	#endregion

	#region Public Methods

	/// <summary>Whether most of their rated players avoid the map solo (same rule as <see cref="MapComfort.IsAvoided"/>, on the DTO).</summary>
	public static bool IsAvoided(MapComfortDto comfort) =>
		comfort.RatedPlayers >= IndividualSignal.MinRatedPlayers
		&& comfort.AvoidingPlayers >= 2
		&& comfort.AvoidingPlayers * 2 > comfort.RatedPlayers
		&& comfort.RegularPlayers <= 1;

	/// <summary>Maps they barely play as a team that most players also avoid solo, for which <see cref="SoloAvoidance"/> speaks.</summary>
	public static HashSet<string> SoloAvoidedMaps(InsightInput input) =>
		(input.Individual?.Theirs.MapComfort ?? [])
			.Where(c => IsAvoided(c) && TeamGames(input, c.MapName) <= OpponentVetoPredictor.SoloBanMaxTeamGames)
			.Select(c => c.MapName)
			.ToHashSet(StringComparer.OrdinalIgnoreCase);

	/// <summary>The most avoided of <see cref="SoloAvoidedMaps"/> (the one <see cref="SoloAvoidance"/> reports), or null.</summary>
	public static MapComfortDto? MostAvoidedMap(InsightInput input)
	{
		var avoided = SoloAvoidedMaps(input);
		return (input.Individual?.Theirs.MapComfort ?? [])
			.Where(c => avoided.Contains(c.MapName))
			.OrderByDescending(c => c.AvoidingPlayers)
			.ThenBy(c => c.SoloShare)
			.FirstOrDefault();
	}

	/// <summary>"4 z 5 graczy unika Ancient także w meczach solo → prawie pewny ban" for the most avoided such map.</summary>
	public static IEnumerable<OpponentInsightDto> SoloAvoidance(InsightInput input)
	{
		var best = MostAvoidedMap(input);

		if (best is not null)
		{
			yield return new(
				"SoloAvoidance",
				"Info",
				$"{best.AvoidingPlayers} z {best.RatedPlayers} graczy unika {best.MapName} także w meczach solo → prawie pewny ban",
				$"drużynowo {TeamGames(input, best.MapName)} z {input.TheirTeamGames} meczów; unikają: {string.Join(", ", best.AvoidingNicknames)}; pewność {(best.RatedPlayers >= 5 ? "wysoka" : "średnia")}");
		}
	}

	/// <summary>A map most of their players play and win solo while team data is thin — a possible pick.</summary>
	public static IEnumerable<OpponentInsightDto> SoloComfortPick(InsightInput input)
	{
		var best = (input.Individual?.Theirs.MapComfort ?? [])
			.Where(c => c.RatedPlayers >= IndividualSignal.MinRatedPlayers
				&& c.RegularPlayers >= 2
				&& c.RegularPlayers * 2 > c.RatedPlayers
				&& c.AvgWinRate >= ComfortPickMinWinRate
				&& TeamGames(input, c.MapName) < OpponentVetoPredictor.MinGamesForPrediction)
			.OrderByDescending(c => c.RegularPlayers)
			.ThenByDescending(c => c.AvgWinRate)
			.FirstOrDefault();

		if (best is not null)
		{
			yield return new(
				"SoloComfort",
				"Info",
				$"{best.RegularPlayers} z {best.RatedPlayers} graczy gra {best.MapName} regularnie solo (śr. {Pct(best.AvgWinRate!.Value)}% wygranych) → możliwy pick",
				$"regularnie: {string.Join(", ", best.RegularNicknames)}; śr. K/D {Num(best.AvgKdRatio ?? 0)}; pewność {(TeamGames(input, best.MapName) == 0 ? "niska" : "średnia")}");
		}
	}

	/// <summary>Their hottest player (K/D clearly up over the last games) and their coldest one (clearly down).</summary>
	public static IEnumerable<OpponentInsightDto> PlayerForm(InsightInput input)
	{
		var players = (input.Individual?.Theirs.Players ?? []).Where(p => p.RecentForm is not null).ToList();
		var hot = players.Where(p => p.RecentForm!.Direction == "Up").OrderByDescending(p => p.RecentForm!.KdDelta).FirstOrDefault();
		var cold = players.Where(p => p.RecentForm!.Direction == "Down").OrderBy(p => p.RecentForm!.KdDelta).FirstOrDefault();

		if (hot is not null)
		{
			var f = hot.RecentForm!;
			yield return new("PlayerForm", "Warning", $"{hot.Nickname} w formie: K/D {Kd(f.RecentKdRatio)} w ostatnich {f.RecentGames} meczach (wcześniej {Kd(f.EarlierKdRatio)})", FormEvidence(f));
		}

		if (cold is not null)
		{
			var f = cold.RecentForm!;
			yield return new("PlayerForm", "Info", $"{cold.Nickname} bez formy: K/D {Kd(f.RecentKdRatio)} w ostatnich {f.RecentGames} meczach (wcześniej {Kd(f.EarlierKdRatio)})", FormEvidence(f));
		}
	}

	/// <summary>Our player with the best solo K/D on a map that isn't our permanent ban — "Nasz X na Inferno: K/D 1.30 solo".</summary>
	public static IEnumerable<OpponentInsightDto> OurStrongMap(InsightInput input)
	{
		var banned = input.Maps.Where(m => m.PoolStatus == "Ban").Select(m => m.MapName).ToHashSet(StringComparer.OrdinalIgnoreCase);
		var best = (input.Individual?.Ours.Players ?? [])
			.SelectMany(p => p.Maps.Select(m => (Player: p, Map: m)))
			.Where(x => x.Map.SoloGames >= StrongMapMinSoloGames && x.Map.SoloKdRatio >= StrongMapMinKd && !banned.Contains(x.Map.MapName))
			.OrderByDescending(x => x.Map.SoloKdRatio)
			.ThenByDescending(x => x.Map.SoloGames)
			.FirstOrDefault();

		if (best.Player is not null)
		{
			yield return new(
				"OurStrongMap",
				"Info",
				$"Nasz {best.Player.Nickname} na {best.Map.MapName}: K/D {Kd(best.Map.SoloKdRatio!.Value)} solo — mocna mapa",
				$"{best.Map.SoloGames} meczów solo, {Pct(best.Map.SoloWinRate ?? 0)}% wygranych; pewność {(best.Map.SoloGames >= 10 ? "wysoka" : "średnia")}");
		}
	}

	#endregion

	#region Private Methods

	/// <summary>Their team games on a map from the matrix row (0 when the row is missing).</summary>
	private static int TeamGames(InsightInput input, string map) =>
		input.Maps.FirstOrDefault(m => string.Equals(m.MapName, map, StringComparison.OrdinalIgnoreCase))?.TheirGames ?? 0;

	/// <summary>Win-rate change and sample size behind a form arrow.</summary>
	private static string FormEvidence(PlayerRecentFormDto f) =>
		$"WR {Pct(f.RecentWinRate)}% (wcześniej {Pct(f.EarlierWinRate)}%), {f.EarlierGames} wcześniejszych meczów; pewność {(f.EarlierGames >= 20 ? "wysoka" : "średnia")}";

	/// <summary>A percentage rounded to a whole number.</summary>
	private static string Pct(double value) => Math.Round(value).ToString("0", CultureInfo.InvariantCulture);

	/// <summary>A number with up to two decimals, invariant culture.</summary>
	private static string Num(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

	/// <summary>A K/D ratio with exactly two decimals.</summary>
	private static string Kd(double value) => value.ToString("0.00", CultureInfo.InvariantCulture);

	#endregion
}
