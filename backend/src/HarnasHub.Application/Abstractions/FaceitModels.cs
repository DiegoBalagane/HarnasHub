namespace HarnasHub.Application.Abstractions;

/// <summary>A FACEIT player profile; <paramref name="Elo"/> and <paramref name="SkillLevel"/> are the CS2 values, null when unknown.</summary>
public record FaceitPlayerInfo(string PlayerId, string Nickname, string? SteamId64, int? Elo, int? SkillLevel);

/// <summary>A player as listed in a team or a match roster.</summary>
public record FaceitPlayerRef(string PlayerId, string Nickname, string? SteamId64, int? SkillLevel);

/// <summary>A FACEIT team and its members.</summary>
public record FaceitTeamInfo(string TeamId, string Name, List<FaceitPlayerRef> Members);

/// <summary>One side of a match room.</summary>
public record FaceitFactionInfo(string FactionId, string Name, List<FaceitPlayerRef> Players);

/// <summary>A match room with both factions.</summary>
public record FaceitMatchInfo(string MatchId, List<FaceitFactionInfo> Factions)
{
	/// <summary>Demo resource URLs of the match (one per map, in map order) — only usable through the Downloads API.</summary>
	public List<string> DemoUrls { get; init; } = [];

	/// <summary>FACEIT's raw competition type (e.g. "matchmaking", "hub", "championship"), null when missing.</summary>
	public string? CompetitionType { get; init; }

	/// <summary>Name of the queue, hub or championship the match belongs to.</summary>
	public string? CompetitionName { get; init; }

	/// <summary>When the match started, null when FACEIT didn't say.</summary>
	public DateTime? StartedAtUtc { get; init; }

	/// <summary>When the match finished, null while it is still running or unknown.</summary>
	public DateTime? FinishedAtUtc { get; init; }

	/// <summary>Raw names of the picked maps in play order (e.g. "de_mirage"); empty when FACEIT has no veto data.</summary>
	public List<string> PickedMaps { get; init; } = [];
}

/// <summary>One entry of a player's match history; <paramref name="Status"/> is FACEIT's raw value (e.g. "FINISHED").</summary>
public record FaceitHistoryItem(
	string MatchId,
	DateTime? FinishedAtUtc,
	string? CompetitionType,
	string? CompetitionName,
	string? Status)
{
	/// <summary>FACEIT competition id (championship, hub or queue id), null when missing.</summary>
	public string? CompetitionId { get; init; }

	/// <summary>Both factions with their team ids and player ids; empty when FACEIT sent none.</summary>
	public List<FaceitHistoryFaction> Factions { get; init; } = [];
}

/// <summary>One faction of a history entry: <paramref name="TeamId"/> is FACEIT's faction id — the FACEIT team id in championship
/// games (e.g. ESEA League), a per-room id in matchmaking.</summary>
public record FaceitHistoryFaction(string TeamId, List<string> PlayerIds);

/// <summary>One player's line on a map scoreboard.</summary>
public record FaceitPlayerMapStats(
	string PlayerId,
	string Nickname,
	int Kills,
	int Deaths,
	int Assists,
	double? Adr,
	double? HeadshotPercent,
	int TripleKills,
	int QuadroKills,
	int PentaKills,
	int Mvps);

/// <summary>One team's result and players on a single map.</summary>
public record FaceitTeamMapStats(string TeamId, string? Name, int Score, bool Won, List<FaceitPlayerMapStats> Players);

/// <summary>The scoreboard of one map of a match; <paramref name="MapName"/> is FACEIT's raw value (e.g. "de_mirage").</summary>
public record FaceitMapStats(int MapNumber, string? MapName, List<FaceitTeamMapStats> Teams);

/// <summary>A player's lifetime numbers on one map from the FACEIT stats "Map" segment; <paramref name="MapName"/> is FACEIT's raw
/// label (e.g. "de_mirage" or "Mirage") and <paramref name="KdRatio"/> the average K/D over those matches.</summary>
public record FaceitLifetimeMapStats(string MapName, int Matches, int Wins, double? KdRatio);
