#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Deeper single-match analysis built from the stored timeline: trades, clutches, opening duels, flashes and
/// (when the map has a library) the match's grenades compared with the team's trained nades. <paramref name="Grenades"/>
/// is null when the map is unknown or the library has nothing pinned on it.</summary>
public record MatchAnalysisDto(
	string? MapName,
	bool HasZones,
	bool OurTeamResolved,
	TradeSummaryDto Trades,
	ClutchSummaryDto Clutches,
	OpeningSummaryDto Openings,
	FlashSummaryDto Flashes,
	GrenadeLibraryComparisonDto? Grenades);

#region Trades

/// <summary>One of our players' trade numbers: kills that avenged a teammate within the trade window and deaths that were
/// avenged. <paramref name="Deaths"/> counts only deaths to enemies.</summary>
public record TradePlayerDto(string SteamId64, string Name, int Deaths, int TradedDeaths, int TradeKills);

/// <summary>Trade totals for both teams (deaths to enemies only) plus our players' rows.</summary>
public record TradeSummaryDto(
	int OurDeaths,
	int OurTradedDeaths,
	int OurTradeKills,
	int OpponentDeaths,
	int OpponentTradedDeaths,
	IReadOnlyList<TradePlayerDto> Players);

#endregion

#region Clutches

/// <summary>One 1vX situation: the last player alive of a team against <paramref name="Versus"/> enemies, and whether
/// that team won the round.</summary>
public record ClutchDto(int RoundNumber, string SteamId64, string Name, int Versus, bool Won, bool IsOurs);

/// <summary>One of our players' clutch record.</summary>
public record ClutchPlayerDto(string SteamId64, string Name, int Attempts, int Won, int BestWonVersus);

/// <summary>Clutch totals for both teams, our players' records and every clutch in round order.</summary>
public record ClutchSummaryDto(
	int OurAttempts,
	int OurWon,
	int OpponentAttempts,
	int OpponentWon,
	IReadOnlyList<ClutchPlayerDto> Players,
	IReadOnlyList<ClutchDto> Clutches);

#endregion

#region Openings

/// <summary>One opening duel we took part in; coordinates are radar fractions (null without calibration).</summary>
public record OpeningDuelDto(
	int RoundNumber,
	MapSide? OurSide,
	bool WonByUs,
	string? Zone,
	string? KillerName,
	string VictimName,
	float? KillerX,
	float? KillerY,
	float? VictimX,
	float? VictimY);

/// <summary>Opening duels won/lost by us on one side in one zone (null zone = outside every known zone).</summary>
public record OpeningBucketDto(MapSide Side, string? Zone, int Won, int Lost);

/// <summary>One of our players' opening duel record.</summary>
public record OpeningPlayerDto(string SteamId64, string Name, int Won, int Lost);

/// <summary>Opening duels of a match: every duel, plus per side+zone and per player rollups.</summary>
public record OpeningSummaryDto(
	IReadOnlyList<OpeningDuelDto> Duels,
	IReadOnlyList<OpeningBucketDto> Buckets,
	IReadOnlyList<OpeningPlayerDto> Players);

#endregion

#region Flashes

/// <summary>One of our players' flash effectiveness: enemies blinded (and for how long on average) vs teammates blinded.</summary>
public record FlashPlayerDto(
	string SteamId64,
	string Name,
	int EnemiesFlashed,
	double AvgEnemyBlindSeconds,
	int TeamFlashes,
	double TeamBlindSeconds);

/// <summary>Flash stats of our team; <paramref name="HasData"/> is false for timelines parsed before blind tracking existed.</summary>
public record FlashSummaryDto(bool HasData, int EnemiesFlashed, int TeamFlashes, IReadOnlyList<FlashPlayerDto> Players);

#endregion

#region Grenades

/// <summary>One pinned library nade and how many times we threw something landing on it in this match.</summary>
public record LibraryNadeUsageDto(Guid Id, string Type, string Title, float LandingX, float LandingY, int TimesThrown);

/// <summary>A grenade spot we threw repeatedly that the library doesn't have, a candidate to add; throw position is the
/// first matching throw's.</summary>
public record UnlistedGrenadeDto(
	string Type,
	float LandingX,
	float LandingY,
	float? ThrowX,
	float? ThrowY,
	int Count,
	string? Zone);

/// <summary>Per-type coverage of the library in this match.</summary>
public record GrenadeTypeCoverageDto(string Type, int Total, int Thrown);

/// <summary>Our match grenades vs the team's pinned nade library on the same map.</summary>
public record GrenadeLibraryComparisonDto(
	int TrainedTotal,
	int TrainedThrown,
	int OurGrenadesThrown,
	IReadOnlyList<GrenadeTypeCoverageDto> ByType,
	IReadOnlyList<LibraryNadeUsageDto> Library,
	IReadOnlyList<UnlistedGrenadeDto> Candidates);

#endregion
