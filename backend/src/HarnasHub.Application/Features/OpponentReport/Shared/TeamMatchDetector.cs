using HarnasHub.Application.Features.MapPool.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>One map a roster played together, seen from that roster's side.</summary>
public record TeamGame(
	string FaceitMatchId,
	DateTime PlayedAtUtc,
	MapName? Map,
	string? RawMapName,
	int RoundsFor,
	int RoundsAgainst,
	bool Won,
	List<string> SidePlayerIds,
	string? CompetitionName);

/// <summary>Decides which cached FACEIT games were played by a roster as a team rather than by its players solo.</summary>
public static class TeamMatchDetector
{
	#region Public Fields

	/// <summary>A game counts as a team game when at least this many roster players were on the same side.</summary>
	public const int MinRosterPlayers = 3;

	#endregion

	#region Public Methods

	/// <summary>The roster's side (1 or 2), or null when neither side has <see cref="MinRosterPlayers"/> of them — or both do
	/// (an internal game between roster members says nothing about the team).</summary>
	public static int? FindSide(IEnumerable<string> team1, IEnumerable<string> team2, IReadOnlySet<string> roster)
	{
		var onTeam1 = team1.Count(roster.Contains);
		var onTeam2 = team2.Count(roster.Contains);
		var team1Qualifies = onTeam1 >= MinRosterPlayers;
		var team2Qualifies = onTeam2 >= MinRosterPlayers;

		return team1Qualifies == team2Qualifies ? null : team1Qualifies ? 1 : 2;
	}

	/// <summary>All team games of <paramref name="roster"/> among <paramref name="matches"/>, newest first.</summary>
	public static List<TeamGame> Detect(IEnumerable<FaceitMatch> matches, IReadOnlySet<string> roster) =>
		matches
			.Select(match => (Match: match, Side: FindSide(match.Team1PlayerIds, match.Team2PlayerIds, roster)))
			.Where(x => x.Side.HasValue)
			.Select(x => ToTeamGame(x.Match, x.Side!.Value))
			.OrderByDescending(g => g.PlayedAtUtc)
			.ToList();

	/// <summary>How many games had some roster players but weren't team games — the "solo" sample behind player comfort; with <paramref name="players"/> only games of those players count (the roster still decides team vs solo).</summary>
	public static int CountSoloGames(IEnumerable<FaceitMatch> matches, IReadOnlySet<string> roster, IReadOnlySet<string>? players = null) =>
		matches.Count(m =>
			m.Team1PlayerIds.Concat(m.Team2PlayerIds).Any((players ?? roster).Contains)
			&& FindSide(m.Team1PlayerIds, m.Team2PlayerIds, roster) is null);

	/// <summary>Pool map of a raw FACEIT map name ("de_mirage" → Mirage); null for maps outside the pool.</summary>
	public static MapName? ParseMap(string? rawMapName)
	{
		if (string.IsNullOrWhiteSpace(rawMapName))
		{
			return null;
		}

		var name = rawMapName.Trim();
		var underscore = name.IndexOf('_');
		return MapNameParser.Parse(underscore >= 0 && underscore < 4 ? name[(underscore + 1)..] : name);
	}

	#endregion

	#region Private Methods

	/// <summary>Flips the stored team-1/team-2 view to the roster's side.</summary>
	private static TeamGame ToTeamGame(FaceitMatch match, int side)
	{
		var roundsFor = side == 1 ? match.Team1Score : match.Team2Score;
		var roundsAgainst = side == 1 ? match.Team2Score : match.Team1Score;
		var won = match.WinnerTeam == 0 ? roundsFor > roundsAgainst : match.WinnerTeam == side;

		return new TeamGame(
			match.FaceitMatchId,
			match.PlayedAtUtc,
			ParseMap(match.MapName),
			match.MapName,
			roundsFor,
			roundsAgainst,
			won,
			side == 1 ? match.Team1PlayerIds : match.Team2PlayerIds,
			match.CompetitionName);
	}

	#endregion
}
