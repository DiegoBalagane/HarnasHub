namespace HarnasHub.Core.Entities;

/// <summary>One map of a cached FACEIT match (a BO3 is up to three rows) with both rosters, so team games can be detected for any roster.</summary>
public class FaceitMatch
{
	#region Public Properties

	public Guid Id { get; set; }
	/// <summary>FACEIT match id (e.g. "1-…"); unique together with <see cref="MapNumber"/>.</summary>
	public string FaceitMatchId { get; set; } = string.Empty;
	/// <summary>1-based map number within the match.</summary>
	public int MapNumber { get; set; }
	public DateTime PlayedAtUtc { get; set; }
	/// <summary>Raw FACEIT map name, e.g. "de_mirage".</summary>
	public string? MapName { get; set; }
	/// <summary>FACEIT competition type, e.g. "matchmaking" or "championship".</summary>
	public string? CompetitionType { get; set; }
	public string? CompetitionName { get; set; }
	public string? Team1Name { get; set; }
	public string? Team2Name { get; set; }
	/// <summary>Rounds won by team 1 on this map.</summary>
	public int Team1Score { get; set; }
	/// <summary>Rounds won by team 2 on this map.</summary>
	public int Team2Score { get; set; }
	/// <summary>1 or 2 for the winning team, 0 when unknown.</summary>
	public int WinnerTeam { get; set; }
	public List<string> Team1PlayerIds { get; set; } = [];
	public List<string> Team2PlayerIds { get; set; } = [];
	public DateTime FetchedAtUtc { get; set; }

	#endregion
}
