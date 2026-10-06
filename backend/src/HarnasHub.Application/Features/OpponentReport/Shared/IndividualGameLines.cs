using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>One linked player's scoreboard line seen from their own side: <paramref name="Map"/> is null off the pool,
/// <paramref name="Won"/> means the player's faction won and <paramref name="TeamGame"/> that the roster played it as a team.</summary>
public record IndividualGameLine(
	string PlayerId,
	string Nickname,
	MapName? Map,
	DateTime PlayedAtUtc,
	bool Won,
	bool TeamGame,
	int Kills,
	int Deaths,
	double? Adr,
	double? HeadshotPercent);

/// <summary>Turns cached matches and scoreboard lines into <see cref="IndividualGameLine"/>s of one roster.</summary>
public static class IndividualGameLines
{
	#region Public Methods

	/// <summary>Lines of <paramref name="roster"/> players in the cached <paramref name="matches"/> (lines of other matches are
	/// dropped, so the window of the matches applies), oldest first. A line is a team game when ≥ 3 roster players stood on the
	/// player's side (<see cref="TeamMatchDetector.FindSide"/>); everything else — including games against other roster members —
	/// is solo.</summary>
	public static List<IndividualGameLine> Build(
		IEnumerable<FaceitMatch> matches,
		IEnumerable<FaceitMatchPlayerStat> stats,
		IReadOnlySet<string> roster)
	{
		var byId = matches.ToDictionary(m => m.Id);
		var sides = new Dictionary<Guid, int?>();

		return stats
			.Where(s => roster.Contains(s.PlayerId) && byId.ContainsKey(s.MatchId))
			.Select(s =>
			{
				var match = byId[s.MatchId];
				if (!sides.TryGetValue(match.Id, out var side))
				{
					side = TeamMatchDetector.FindSide(match.Team1PlayerIds, match.Team2PlayerIds, roster);
					sides[match.Id] = side;
				}

				return new IndividualGameLine(
					s.PlayerId,
					s.Nickname,
					TeamMatchDetector.ParseMap(match.MapName),
					match.PlayedAtUtc,
					Won(match, s.Team),
					side == s.Team,
					s.Kills,
					s.Deaths,
					s.Adr,
					s.HeadshotPercent);
			})
			.OrderBy(l => l.PlayedAtUtc)
			.ToList();
	}

	#endregion

	#region Private Methods

	/// <summary>Whether faction <paramref name="team"/> (1 or 2) won; falls back to the score when FACEIT reports no winner.</summary>
	private static bool Won(FaceitMatch match, int team)
	{
		if (match.WinnerTeam != 0)
		{
			return match.WinnerTeam == team;
		}

		return team == 1 ? match.Team1Score > match.Team2Score : match.Team2Score > match.Team1Score;
	}

	#endregion
}
