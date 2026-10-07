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
	List<string> VetoReasons)
{
	/// <summary>Our win rate smoothed towards 50% (percent) — shown next to the raw one; null without games.</summary>
	public double? OurSmoothedWinRate { get; init; }

	/// <summary>Their recency-weighted, smoothed win rate (percent) the decisions use; null without games.</summary>
	public double? TheirSmoothedWinRate { get; init; }

	/// <summary>Our FACEIT team-game wins on the map (part of <see cref="MapComparisonDto.OurWins"/>); null in older snapshots.</summary>
	public int? OurFaceitWins { get; init; }

	/// <summary>Our players' solo-form win-rate prior (0.4–0.6) fed to the veto score as a small secondary term; null without data.</summary>
	public double? OurSoloPrior { get; init; }

	/// <summary>Whether our lineup plays the map regularly solo (veto familiarity, <c>VetoFamiliarity.PlaysIndividually</c>); null in older snapshots.</summary>
	public bool? OurPlaysIndividually { get; init; }

	/// <summary>True when our sample is below <see cref="SampleThresholds.MinGamesForWinRate"/> — "za mało danych", not used to decide.</summary>
	public bool OurLowSample { get; init; }

	/// <summary>True when their sample is below <see cref="SampleThresholds.MinGamesForWinRate"/>.</summary>
	public bool TheirLowSample { get; init; }

	/// <summary>Lifetime FACEIT numbers of their active lineup on the map; null without data (and in older snapshots).</summary>
	public MapLifetimeDto? TheirLifetime { get; init; }

	/// <summary>Lifetime FACEIT numbers of our linked players on the map; null without data.</summary>
	public MapLifetimeDto? OurLifetime { get; init; }

	/// <summary>Their official team games on the map (faction = their FACEIT team, e.g. ESEA), part of <see cref="MapComparisonDto.TheirGames"/>; null in older snapshots.</summary>
	public int? TheirOfficialGames { get; init; }

	/// <summary>Their other games on the map with ≥ 3 of the lineup on one side, part of <see cref="MapComparisonDto.TheirGames"/>; null in older snapshots.</summary>
	public int? TheirTogetherGames { get; init; }
}

/// <summary>A lineup's summed lifetime FACEIT numbers on one map, like the match room's aggregate: <paramref name="Matches"/> of all
/// <paramref name="Players"/>, win rate and share (percent of their lifetime pool matches), average K/D; <paramref name="Experienced"/>
/// marks a map they play a lot individually.</summary>
public record MapLifetimeDto(int Players, int Matches, double? WinRate, double? AvgKdRatio, double Share, bool Experienced);

/// <summary>One opponent player in the lineup header: team games in the lineup window and overall (for an ESEA season lineup: the
/// season's league matches and official matches overall), last team game.</summary>
public record LineupPlayerDto(
	string PlayerId,
	string Nickname,
	int? Elo,
	int? SkillLevel,
	int RecentTeamGames,
	int TeamGames,
	DateTime? LastTeamGameAtUtc);

/// <summary>Who the report treats as the opponent's active lineup and why (<paramref name="Basis"/>); <paramref name="Inactive"/> are
/// linked ex-members and subs, excluded from every player-facing number. With an ESEA season lineup <paramref name="WindowGames"/>
/// is the number of the season's league matches and each player's <see cref="LineupPlayerDto.RecentTeamGames"/> their appearances in them.</summary>
public record ActiveLineupDto(string Basis, int WindowGames, List<LineupPlayerDto> Active, List<LineupPlayerDto> Inactive)
{
	/// <summary>Where the lineup comes from: "EseaSeason", "OfficialMatches" (team games without ESEA) or "TeamGames"
	/// (≥ 3 linked players together); null in older snapshots.</summary>
	public string? Source { get; init; }

	/// <summary>Label of the ESEA season the lineup comes from ("S59"); null for other sources.</summary>
	public string? Season { get; init; }

	/// <summary>Competition name of the season's latest league match (e.g. "S59 EU Open10 D - Regular Season"); null for other sources.</summary>
	public string? SeasonCompetition { get; init; }

	/// <summary>Official matches of the FACEIT team (all seasons and competitions) in the window; null without a linked team.</summary>
	public int? OfficialMatches { get; init; }
}

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

	/// <summary>Individual form of both rosters (team and solo games) with per-map comfort; null in snapshots generated before
	/// it existed, so old snapshots stay readable.</summary>
	public IndividualFormDto? IndividualForm { get; init; }

	/// <summary>The opponent's active lineup and the linked players left out of it; null in snapshots generated before it existed.</summary>
	public ActiveLineupDto? ActiveLineup { get; init; }

	/// <summary>Roster players without a FACEIT account found (with the reason), filled live on every read; null in older payloads.</summary>
	public List<UnresolvedRosterPlayerDto>? UnresolvedOurPlayers { get; init; }
}

/// <summary>Outcome of a sync run: how many map games were newly cached and whether the per-run fetch cap cut it short.</summary>
public record FaceitSyncResultDto(int Players, int NewMapGames, bool Complete);
