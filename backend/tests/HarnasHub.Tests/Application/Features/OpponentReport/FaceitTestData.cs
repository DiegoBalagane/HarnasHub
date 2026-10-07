using HarnasHub.Core.Entities;

namespace HarnasHub.Tests.Application.Features.OpponentReport;

/// <summary>Builders for cached FACEIT rows shared by the opponent report tests.</summary>
public static class FaceitTestData
{
	#region Public Fields

	/// <summary>Five opponent player ids.</summary>
	public static readonly string[] Them = ["t1", "t2", "t3", "t4", "t5"];

	/// <summary>Five of our player ids.</summary>
	public static readonly string[] Us = ["u1", "u2", "u3", "u4", "u5"];

	#endregion

	#region Public Methods

	/// <summary>A cached map: <paramref name="team1"/> scored <paramref name="team1Score"/>, the other side <paramref name="team2Score"/>.</summary>
	public static FaceitMatch Match(
		string map,
		IEnumerable<string> team1,
		IEnumerable<string> team2,
		int team1Score,
		int team2Score,
		DateTime playedAtUtc,
		string? matchId = null) =>
		new()
		{
			Id = Guid.NewGuid(),
			FaceitMatchId = matchId ?? Guid.NewGuid().ToString(),
			MapNumber = 1,
			PlayedAtUtc = playedAtUtc,
			MapName = map,
			Team1Score = team1Score,
			Team2Score = team2Score,
			WinnerTeam = team1Score > team2Score ? 1 : team2Score > team1Score ? 2 : 0,
			Team1PlayerIds = team1.ToList(),
			Team2PlayerIds = team2.ToList(),
			FetchedAtUtc = playedAtUtc
		};

	/// <summary>A cached championship map where team 1's faction is the FACEIT team <paramref name="teamId"/> (as in ESEA League).</summary>
	public static FaceitMatch Official(
		string teamId,
		string competitionName,
		IEnumerable<string> team1,
		DateTime playedAtUtc,
		string? matchId = null,
		string map = "de_mirage",
		bool won = true)
	{
		var match = Match(map, team1, Strangers(), won ? 13 : 7, won ? 7 : 13, playedAtUtc, matchId);
		match.CompetitionType = "championship";
		match.CompetitionName = competitionName;
		match.Team1FactionId = teamId;
		match.Team2FactionId = Guid.NewGuid().ToString();
		return match;
	}

	/// <summary>Five random strangers for the other side of a game.</summary>
	public static string[] Strangers() => Enumerable.Range(0, 5).Select(_ => Guid.NewGuid().ToString()).ToArray();

	/// <summary>A scoreboard line on a cached map.</summary>
	public static FaceitMatchPlayerStat Stat(FaceitMatch match, string playerId, int kills, int deaths, double? adr) =>
		new()
		{
			Id = Guid.NewGuid(),
			MatchId = match.Id,
			PlayerId = playerId,
			Nickname = $"nick-{playerId}",
			Team = match.Team1PlayerIds.Contains(playerId) ? 1 : 2,
			Kills = kills,
			Deaths = deaths,
			Adr = adr
		};

	#endregion
}
