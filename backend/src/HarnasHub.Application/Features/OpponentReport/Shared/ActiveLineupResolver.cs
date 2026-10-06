using HarnasHub.Core.Entities;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The opponent's active lineup: the ids everything player-facing is limited to, plus the DTO shown in the report header.</summary>
public record ActiveLineup(IReadOnlySet<string> ActiveIds, ActiveLineupDto Dto);

/// <summary>Works out who actually plays for the opponent now. A FACEIT team page lists every member ever (ex-players, subs), so the
/// linked list is only used to detect team games; the lineup is whoever stood on the team's side in its recent team games.</summary>
public static class ActiveLineupResolver
{
	#region Public Fields

	/// <summary>The latest this many team games always form the lineup window…</summary>
	public const int RecentTeamGames = 10;

	/// <summary>…extended by every team game in this many days.</summary>
	public const int RecentDays = 60;

	/// <summary>A player must appear in at least this fraction of the window's games (min. 1) — one stand-in game makes a sub, not a member.</summary>
	public const double MinAppearanceShare = 0.2;

	/// <summary>A linked list of at most this many players is taken as the lineup as is (picked by hand, nothing to filter).</summary>
	public const int MaxLineupSize = 5;

	#endregion

	#region Public Methods

	/// <summary>Resolves the lineup from <paramref name="newestFirst"/> team games of the full linked <paramref name="roster"/>.</summary>
	public static ActiveLineup Resolve(
		IReadOnlyList<TeamGame> newestFirst,
		IReadOnlySet<string> roster,
		DateTime nowUtc,
		IReadOnlyDictionary<string, string> nicknames,
		IReadOnlyList<FaceitPlayerDto> profiles)
	{
		var window = newestFirst
			.Where((game, index) => index < RecentTeamGames || game.PlayedAtUtc >= nowUtc.AddDays(-RecentDays))
			.ToList();
		var minAppearances = Math.Max(1, (int)Math.Ceiling(window.Count * MinAppearanceShare));
		var recent = Appearances(window);

		var (active, basis) = roster.Count <= MaxLineupSize
			? (roster.ToHashSet(), $"Wszyscy powiązani gracze ({roster.Count}) — nie ma kogo odfiltrować")
			: window.Count == 0
				? (roster.ToHashSet(), "Brak meczów drużynowych w oknie — brani są wszyscy powiązani gracze")
				: (roster.Where(id => recent.GetValueOrDefault(id) >= minAppearances).ToHashSet(),
					$"Skład z ostatnich {window.Count} meczów drużynowych (ostatnie {RecentTeamGames} lub z {RecentDays} dni) — gracze obecni w co najmniej {minAppearances} z nich");

		var all = Appearances(newestFirst);
		var players = roster
			.Select(id => ToPlayer(id, recent, all, newestFirst, nicknames, profiles))
			.OrderByDescending(p => p.RecentTeamGames)
			.ThenByDescending(p => p.LastTeamGameAtUtc)
			.ThenBy(p => p.Nickname, StringComparer.OrdinalIgnoreCase)
			.ToList();

		return new ActiveLineup(
			active,
			new ActiveLineupDto(
				basis,
				window.Count,
				players.Where(p => active.Contains(p.PlayerId)).ToList(),
				players.Where(p => !active.Contains(p.PlayerId)).ToList()));
	}

	/// <summary>Only the lines of <paramref name="players"/> — keeps ex-members and subs out of player-facing numbers.</summary>
	public static List<FaceitMatchPlayerStat> Filter(IEnumerable<FaceitMatchPlayerStat> stats, IReadOnlySet<string> players) =>
		stats.Where(s => players.Contains(s.PlayerId)).ToList();

	#endregion

	#region Private Methods

	/// <summary>Player id → number of games in which they stood on the team's side.</summary>
	private static Dictionary<string, int> Appearances(IEnumerable<TeamGame> games) =>
		games.SelectMany(g => g.SidePlayerIds.Distinct()).GroupBy(id => id).ToDictionary(g => g.Key, g => g.Count());

	/// <summary>One lineup entry with its appearance counts and last team game.</summary>
	private static LineupPlayerDto ToPlayer(
		string id,
		Dictionary<string, int> recent,
		Dictionary<string, int> all,
		IReadOnlyList<TeamGame> newestFirst,
		IReadOnlyDictionary<string, string> nicknames,
		IReadOnlyList<FaceitPlayerDto> profiles)
	{
		var profile = profiles.FirstOrDefault(p => p.PlayerId == id);
		return new LineupPlayerDto(
			id,
			nicknames.GetValueOrDefault(id) ?? profile?.Nickname ?? id,
			profile?.Elo,
			profile?.SkillLevel,
			recent.GetValueOrDefault(id),
			all.GetValueOrDefault(id),
			newestFirst.FirstOrDefault(g => g.SidePlayerIds.Contains(id))?.PlayedAtUtc);
	}

	#endregion
}
