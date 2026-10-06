using HarnasHub.Application.Features.OpponentReport.Tendencies;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>A cached FACEIT player as shown on the report.</summary>
public record FaceitPlayerDto(string PlayerId, string Nickname, int? Elo, int? SkillLevel);

/// <summary>The FACEIT roster an opponent is linked to.</summary>
public record OpponentFaceitLinkDto(
	string OpponentName,
	string? FaceitTeamId,
	List<FaceitPlayerDto> Players,
	DateTime LinkedAtUtc,
	DateTime? LastSyncedAtUtc);

/// <summary>One TL;DR point: <paramref name="Kind"/> names the rule, <paramref name="Severity"/> is "High", "Warning" or "Info",
/// <paramref name="Text"/> is the Polish sentence and <paramref name="Evidence"/> the numbers behind it.</summary>
public record OpponentInsightDto(string Kind, string Severity, string Text, string Evidence);

/// <summary>One row of the map matrix. Rates, shares, trend and advantage are percentages / percentage points; nullable rates
/// are null without games. <paramref name="Confidence"/> is "Low", "Medium" or "High"; <paramref name="Prediction"/> is the
/// opponent's expected move ("Ban", "Pick", "Neutral", "Unknown"); <paramref name="Recommendation"/> is ours from <c>VetoScoring</c>.</summary>
public record MapComparisonDto(
	string MapName,
	int TheirGames,
	int TheirWins,
	double? TheirWinRate,
	double? TheirAvgRoundDiff,
	double TheirShare,
	DateTime? TheirLastPlayedAtUtc,
	double? TheirTrend,
	int OurGames,
	double OurWins,
	double? OurWinRate,
	int OurFaceitGames,
	int OurInternalGames,
	string? PoolStatus,
	double Advantage,
	string Confidence,
	string Prediction,
	string PredictionReason,
	int VetoScore,
	string Recommendation,
	List<string> VetoReasons);

/// <summary>One step of a simulated veto; <paramref name="Actor"/>/<paramref name="Action"/> use the <c>VetoActor</c>/<c>VetoAction</c> names.</summary>
public record VetoPlanStepDto(int Order, string Actor, string Action, string MapName, string Reason);

/// <summary>A full simulated veto for one format ("Bo1" or "Bo3"), assuming we start.</summary>
public record VetoPlanDto(string Format, List<VetoPlanStepDto> Steps);

/// <summary>An opponent player's numbers on one map, across team and solo games in the window.</summary>
public record PlayerToWatchDto(
	string PlayerId,
	string Nickname,
	int Games,
	double KdRatio,
	double? Adr,
	double? HeadshotPercent,
	double MultiKillsPerGame);

/// <summary>The opponent players to keep an eye on for one map, most dangerous first.</summary>
public record MapPlayersToWatchDto(string MapName, List<PlayerToWatchDto> Players);

/// <summary>One recent team game of the opponent; <paramref name="MapName"/> is the pool map name or FACEIT's raw one.</summary>
public record FormGameDto(
	string FaceitMatchId,
	DateTime PlayedAtUtc,
	string? MapName,
	int RoundsFor,
	int RoundsAgainst,
	bool Won,
	string? CompetitionName);

/// <summary>The opponent's recent form: last team games, current streak ("W3"/"L2") and players new in the latest games.</summary>
public record OpponentFormDto(List<FormGameDto> LastGames, string? Streak, List<string> NewPlayers);

/// <summary>The "them vs us" report for one opponent built from cached FACEIT data plus our internal results and map pool.
/// <paramref name="FaceitConfigured"/> and the next-event fields are filled live on every read; the rest comes from the snapshot.</summary>
public record OpponentReportDto(
	string OpponentName,
	bool FaceitConfigured,
	OpponentFaceitLinkDto? Link,
	DateTime GeneratedAtUtc,
	DateTime? DataSyncedAtUtc,
	int TheirTeamGames,
	int TheirSoloGames,
	int OurTeamGames,
	int OurInternalGames,
	int OurLinkedPlayers,
	List<OpponentInsightDto> Insights,
	List<MapComparisonDto> Maps,
	List<VetoPlanDto> VetoPlans,
	List<MapPlayersToWatchDto> PlayersToWatch,
	OpponentFormDto Form,
	Guid? NextEventId,
	DateTime? NextEventAtUtc)
{
	/// <summary>Tendencies from the opponent's analysed demos per map (filled live on every read, never stored in the
	/// snapshot); empty without demos, so a FACEIT-only report is unchanged.</summary>
	public List<MapTendenciesDto> Tendencies { get; init; } = [];
}

/// <summary>Outcome of a sync run: how many map games were newly cached and whether the per-run fetch cap cut it short.</summary>
public record FaceitSyncResultDto(int Players, int NewMapGames, bool Complete);
