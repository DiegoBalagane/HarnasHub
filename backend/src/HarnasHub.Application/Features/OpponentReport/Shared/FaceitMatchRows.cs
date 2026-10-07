#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Turns fetched FACEIT data into cached <see cref="FaceitMatch"/> rows (one per map) with scoreboard lines, and fills the
/// faction/competition ids of rows cached before those were stored.</summary>
public static class FaceitMatchRows
{
	#region Public Methods

	/// <summary>Adds one <see cref="FaceitMatch"/> row per map plus its scoreboard lines; returns how many maps were added.</summary>
	public static int Add(IApplicationDbContext dbContext, FaceitHistoryItem item, IReadOnlyList<FaceitMapStats> maps, DateTime nowUtc)
	{
		var complete = maps.Where(m => m.Teams.Count == 2).ToList();
		foreach (var map in complete)
		{
			var (team1, team2) = (map.Teams[0], map.Teams[1]);
			var team1Ids = team1.Players.Select(p => p.PlayerId).ToList();
			var team2Ids = team2.Players.Select(p => p.PlayerId).ToList();
			var match = new FaceitMatch
			{
				Id = Guid.NewGuid(),
				FaceitMatchId = item.MatchId,
				MapNumber = map.MapNumber,
				PlayedAtUtc = item.FinishedAtUtc ?? nowUtc,
				MapName = map.MapName,
				CompetitionType = item.CompetitionType,
				CompetitionName = item.CompetitionName,
				CompetitionId = item.CompetitionId,
				Team1FactionId = FactionId(team1Ids, item.Factions) ?? StatsTeamId(team1.TeamId),
				Team2FactionId = FactionId(team2Ids, item.Factions) ?? StatsTeamId(team2.TeamId),
				Team1Name = team1.Name,
				Team2Name = team2.Name,
				Team1Score = team1.Score,
				Team2Score = team2.Score,
				WinnerTeam = team1.Won ? 1 : team2.Won ? 2 : 0,
				Team1PlayerIds = team1Ids,
				Team2PlayerIds = team2Ids,
				FetchedAtUtc = nowUtc
			};
			dbContext.FaceitMatches.Add(match);

			foreach (var (team, side) in new[] { (team1, 1), (team2, 2) })
			{
				dbContext.FaceitMatchPlayerStats.AddRange(team.Players.Select(p => new FaceitMatchPlayerStat
				{
					Id = Guid.NewGuid(),
					MatchId = match.Id,
					PlayerId = p.PlayerId,
					Nickname = p.Nickname,
					Team = side,
					Kills = p.Kills,
					Deaths = p.Deaths,
					Assists = p.Assists,
					Adr = p.Adr,
					HeadshotPercent = p.HeadshotPercent,
					TripleKills = p.TripleKills,
					QuadroKills = p.QuadroKills,
					PentaKills = p.PentaKills,
					Mvps = p.Mvps
				}));
			}
		}

		return complete.Count;
	}

	/// <summary>Fills missing faction and competition ids of cached <paramref name="rows"/> from their history entry (no extra
	/// FACEIT call); returns how many rows changed. The caller saves changes.</summary>
	public static int Backfill(IEnumerable<FaceitMatch> rows, FaceitHistoryItem item)
	{
		var changed = 0;
		foreach (var row in rows.Where(r => r.FaceitMatchId == item.MatchId))
		{
			var team1 = row.Team1FactionId ?? FactionId(row.Team1PlayerIds, item.Factions);
			var team2 = row.Team2FactionId ?? FactionId(row.Team2PlayerIds, item.Factions);
			var competition = row.CompetitionId ?? item.CompetitionId;
			if (team1 == row.Team1FactionId && team2 == row.Team2FactionId && competition == row.CompetitionId)
			{
				continue;
			}

			(row.Team1FactionId, row.Team2FactionId, row.CompetitionId) = (team1, team2, competition);
			changed++;
		}

		return changed;
	}

	/// <summary>The id of the history faction sharing the most players with <paramref name="playerIds"/>; null without overlap, so
	/// the scoreboard's team order never has to match FACEIT's faction1/faction2 order.</summary>
	public static string? FactionId(IReadOnlyCollection<string> playerIds, IReadOnlyList<FaceitHistoryFaction> factions) =>
		factions
			.Select(f => (f.TeamId, Overlap: f.PlayerIds.Count(playerIds.Contains)))
			.Where(f => f.Overlap > 0)
			.OrderByDescending(f => f.Overlap)
			.Select(f => f.TeamId)
			.FirstOrDefault();

	#endregion

	#region Private Methods

	/// <summary>The scoreboard's team id — FACEIT sends the faction id there — unless it is the reader's "team1"/"team2" placeholder.</summary>
	private static string? StatsTeamId(string teamId) =>
		string.IsNullOrWhiteSpace(teamId) || (teamId.Length == 5 && teamId.StartsWith("team", StringComparison.Ordinal)) ? null : teamId;

	#endregion
}
