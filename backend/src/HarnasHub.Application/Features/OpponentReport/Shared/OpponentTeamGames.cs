#region Usings

using HarnasHub.Core.Entities;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>The opponent's team games and lineup: <paramref name="Games"/> (newest first) feed every team metric,
/// <paramref name="Lineup"/> limits everything player-facing.</summary>
public record TeamGameSelection(List<TeamGame> Games, ActiveLineup Lineup);

/// <summary>Decides which cached games describe the opponent as a team and who its lineup is. With a linked FACEIT team that has
/// official games (faction = team id, <see cref="OfficialMatchDetector"/>) the lineup is the current ESEA season's
/// (<see cref="SeasonLineupResolver"/>), otherwise who played the team's recent official games; without official games it falls
/// back to <see cref="ActiveLineupResolver"/> over games of ≥ 3 linked players. The team games are then every official game plus
/// every other game with ≥ 3 of the lineup on one side — team members outside the lineup ("randoms") never make a game a team game.</summary>
public static class OpponentTeamGames
{
	#region Public Fields

	/// <summary><see cref="ActiveLineupDto.Source"/> of a lineup from official games outside ESEA (cups, team hubs).</summary>
	public const string OfficialMatchesSource = "OfficialMatches";

	/// <summary><see cref="ActiveLineupDto.Source"/> of a lineup from games of ≥ 3 linked players (no official games).</summary>
	public const string TeamGamesSource = "TeamGames";

	#endregion

	#region Public Methods

	/// <summary>Selects the team games and lineup of the opponent linked to <paramref name="faceitTeamId"/> (may be null) and
	/// <paramref name="roster"/>; nicknames and profiles only label the lineup DTO.</summary>
	public static TeamGameSelection Select(
		IReadOnlyCollection<FaceitMatch> matches,
		string? faceitTeamId,
		IReadOnlySet<string> roster,
		DateTime nowUtc,
		IReadOnlyDictionary<string, string>? nicknames = null,
		IReadOnlyList<FaceitPlayerDto>? profiles = null)
	{
		var names = nicknames ?? new Dictionary<string, string>();
		var official = OfficialMatchDetector.Detect(matches, faceitTeamId);
		var officialMatches = official.Count == 0 ? (int?)null : official.Select(g => g.FaceitMatchId).Distinct().Count();

		var lineup = official.Count == 0
			? WithSource(ActiveLineupResolver.Resolve(TeamMatchDetector.Detect(matches, roster), roster, nowUtc, names, profiles ?? []), TeamGamesSource, null)
			: SeasonLineupResolver.Resolve(official, roster, names, profiles ?? [])
				?? WithSource(
					ActiveLineupResolver.Resolve(official, roster, nowUtc, names, profiles ?? [], "meczów oficjalnych drużyny"),
					OfficialMatchesSource,
					officialMatches);

		return new TeamGameSelection(Combine(matches, official, lineup.ActiveIds), lineup);
	}

	/// <summary><paramref name="official"/> games plus the other games with ≥ 3 of <paramref name="lineup"/> on one side, newest first.</summary>
	public static List<TeamGame> Combine(IEnumerable<FaceitMatch> matches, IReadOnlyCollection<TeamGame> official, IReadOnlySet<string> lineup)
	{
		var officialMatchIds = official.Select(g => g.FaceitMatchId).ToHashSet();
		var together = TeamMatchDetector.Detect(matches.Where(m => !officialMatchIds.Contains(m.FaceitMatchId)), lineup);
		return official.Concat(together).OrderByDescending(g => g.PlayedAtUtc).ToList();
	}

	#endregion

	#region Private Methods

	/// <summary>The lineup with its DTO tagged with the source and the official match count.</summary>
	private static ActiveLineup WithSource(ActiveLineup lineup, string source, int? officialMatches) =>
		lineup with { Dto = lineup.Dto with { Source = source, OfficialMatches = officialMatches } };

	#endregion
}
