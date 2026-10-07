namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The opponent's lineup from its current ESEA League season: whoever played for the FACEIT team in that season's league
/// matches, with appearance counts — FACEIT team members who didn't (ex-players, friends added to the team) are left out.</summary>
public static class SeasonLineupResolver
{
	#region Public Fields

	/// <summary><see cref="ActiveLineupDto.Source"/> of a lineup taken from the current ESEA season.</summary>
	public const string EseaSeasonSource = "EseaSeason";

	#endregion

	#region Public Methods

	/// <summary>The latest ESEA season among <paramref name="official"/> games (highest season number), or null without ESEA games.</summary>
	public static int? CurrentSeason(IEnumerable<TeamGame> official) =>
		official.Max(g => g.Season);

	/// <summary>Lineup of the current season's league matches; null when <paramref name="newestFirstOfficial"/> has no ESEA game.
	/// Appearances count matches, not maps (a BO3 is one appearance); <paramref name="roster"/> members who didn't play are inactive.</summary>
	public static ActiveLineup? Resolve(
		IReadOnlyList<TeamGame> newestFirstOfficial,
		IReadOnlySet<string> roster,
		IReadOnlyDictionary<string, string> nicknames,
		IReadOnlyList<FaceitPlayerDto> profiles)
	{
		if (CurrentSeason(newestFirstOfficial) is not { } season)
		{
			return null;
		}

		var seasonGames = newestFirstOfficial.Where(g => g.Season == season).ToList();
		var seasonMatches = seasonGames.Select(g => g.FaceitMatchId).Distinct().Count();
		var inSeason = MatchAppearances(seasonGames);
		var overall = MatchAppearances(newestFirstOfficial);
		var active = inSeason.Keys.ToHashSet();
		var label = EseaSeasonParser.Label(season);

		var players = active.Concat(roster).Distinct()
			.Select(id => ActiveLineupResolver.ToPlayer(id, inSeason, overall, newestFirstOfficial, nicknames, profiles))
			.OrderByDescending(p => p.RecentTeamGames)
			.ThenByDescending(p => p.TeamGames)
			.ThenBy(p => p.Nickname, StringComparer.OrdinalIgnoreCase)
			.ToList();

		var dto = new ActiveLineupDto(
			$"Skład z sezonu ESEA {label} — gracze, którzy zagrali dla drużyny w meczach ligowych tego sezonu (mecze: {seasonMatches})",
			seasonMatches,
			players.Where(p => active.Contains(p.PlayerId)).ToList(),
			players.Where(p => !active.Contains(p.PlayerId)).ToList())
		{
			Source = EseaSeasonSource,
			Season = label,
			SeasonCompetition = seasonGames[0].CompetitionName,
			OfficialMatches = newestFirstOfficial.Select(g => g.FaceitMatchId).Distinct().Count()
		};
		return new ActiveLineup(active, dto);
	}

	#endregion

	#region Private Methods

	/// <summary>Player id → number of distinct matches in which they stood on the team's side.</summary>
	private static Dictionary<string, int> MatchAppearances(IEnumerable<TeamGame> games) =>
		games
			.GroupBy(g => g.FaceitMatchId)
			.SelectMany(match => match.SelectMany(g => g.SidePlayerIds).Distinct())
			.GroupBy(id => id)
			.ToDictionary(g => g.Key, g => g.Count());

	#endregion
}
