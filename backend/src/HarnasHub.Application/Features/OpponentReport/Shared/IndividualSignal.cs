namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>How the individual (solo) signal from <see cref="MapComfort"/> is blended into the team-based numbers. The rule
/// everywhere is the same shrinkage: the solo signal gets weight <c>w = k / (k + n)</c> with <c>k = </c><see cref="PriorGames"/>,
/// where <c>n</c> is the amount of team evidence about the map — so it dominates without team games and fades as they pile up.</summary>
public static class IndividualSignal
{
	#region Public Fields

	/// <summary>The <c>k</c> of the shrinkage — the solo signal is worth this many team games.</summary>
	public const double PriorGames = 5;

	/// <summary>The solo signal is ignored unless at least this many players have enough solo games to be rated.</summary>
	public const int MinRatedPlayers = 3;

	/// <summary>Cap of the win-rate prior: it may move away from 50% by at most this much, so solo form stays a secondary term.</summary>
	public const double MaxPriorShift = 0.1;

	/// <summary>A win-rate prior is only taken from at least this many regulars on the map.</summary>
	public const int MinRegularsForPrior = 2;

	#endregion

	#region Public Methods

	/// <summary>Whether the comfort has enough rated players to be used at all.</summary>
	public static bool IsUsable(MapComfort? comfort) => comfort is not null && comfort.RatedPlayers >= MinRatedPlayers;

	/// <summary>Solo weight <c>k / (k + n)</c> with <c>n = max(teamGamesOnMap, totalTeamGames / poolSize)</c>: not playing a map in many
	/// team games is evidence too, so every map counts as "seen" at least the uniform share of the team games.</summary>
	public static double Weight(int teamGamesOnMap, int totalTeamGames, int poolSize)
	{
		var evidence = Math.Max(teamGamesOnMap, poolSize <= 0 ? 0 : (double)totalTeamGames / poolSize);
		return PriorGames / (PriorGames + evidence);
	}

	/// <summary>The opponent's solo preference on the same scale as the team one (share + smoothed win rate): average solo share
	/// plus the regulars' smoothed solo win rate, and 0 when nobody plays the map regularly — like a map never played as a team.</summary>
	public static double Preference(MapComfort comfort) =>
		comfort.RegularPlayers == 0 ? 0 : comfort.SoloShare + (comfort.AvgSmoothedWinRate ?? 0.5);

	/// <summary><c>(1 − w)·teamPreference + w·soloPreference</c>; the team preference unchanged when the comfort isn't usable.</summary>
	public static double BlendPreference(double teamPreference, int teamGamesOnMap, int totalTeamGames, int poolSize, MapComfort? comfort)
	{
		if (!IsUsable(comfort))
		{
			return teamPreference;
		}

		var weight = Weight(teamGamesOnMap, totalTeamGames, poolSize);
		return (1 - weight) * teamPreference + weight * Preference(comfort!);
	}

	/// <summary>Win-rate prior for <see cref="MapAdvantage.SmoothedWinRate"/>: the regulars' smoothed solo win rate clamped to
	/// 50% ± <see cref="MaxPriorShift"/>, or 0.5 without enough data. Used as the prior of <c>(wins + k·prior) / (games + k)</c>, its
	/// weight is exactly <c>k / (k + games)</c>, so it can move a smoothed win rate by at most 10 pp (no games) and 5 pp at 5 games.</summary>
	public static double WinRatePrior(MapComfort? comfort)
	{
		if (!IsUsable(comfort) || comfort!.RegularPlayers < MinRegularsForPrior || comfort.AvgSmoothedWinRate is not { } rate)
		{
			return 0.5;
		}

		return Math.Clamp(rate, 0.5 - MaxPriorShift, 0.5 + MaxPriorShift);
	}

	#endregion
}
