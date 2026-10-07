#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>A match's round-by-round timeline as the match page shows it. <paramref name="OurTeamResolved"/> false means
/// "us" couldn't be determined: then "our" fields describe the team that started on T, and the UI labels them neutrally.</summary>
public record MatchTimelineDto(
	string? MapName,
	bool HasZones,
	bool OurTeamResolved,
	int ParserVersion,
	IReadOnlyList<MatchRoundDto> Rounds)
{
	/// <summary>Players the coach/manager hid from this match's analysis (restorable from the match page).</summary>
	public IReadOnlyList<ExcludedPlayerDto> ExcludedPlayers { get; init; } = [];
}

/// <summary>A manually excluded player: SteamID64 as a string (exceeds JS safe integers) and the best known name.</summary>
public record ExcludedPlayerDto(string SteamId64, string Name);

/// <summary>One round: winner, our side and result, running score after it, both teams' buys, bomb and every kill.</summary>
public record MatchRoundDto(
	int Number,
	MapSide? WinnerSide,
	MapSide? OurSide,
	bool? WeWon,
	int OurScoreAfter,
	int OpponentScoreAfter,
	DemoRoundEndReason EndReason,
	float? DurationSeconds,
	MatchTeamEconomyDto? OurEconomy,
	MatchTeamEconomyDto? OpponentEconomy,
	MatchBombDto? Bomb,
	IReadOnlyList<MatchKillDto> Kills);

/// <summary>One team's buy in a round.</summary>
public record MatchTeamEconomyDto(MapSide Side, int EquipmentValue, int Money, int Players, BuyType BuyType);

/// <summary>Bomb plant (and defuse, if any) of a round; <paramref name="Site"/> is null when neither the game nor the
/// map zones could tell which site.</summary>
public record MatchBombDto(
	DemoBombSite? Site,
	float PlantSecondsIntoRound,
	string? PlanterName,
	bool Defused,
	float? DefuseSecondsIntoRound,
	string? DefuserName);

/// <summary>One kill as listed under an expanded round. <paramref name="ByUs"/> is null for world/team kills or when
/// our side is unknown; zones are null when the map has none.</summary>
public record MatchKillDto(
	float SecondsIntoRound,
	string? KillerName,
	string? KillerSteamId64,
	MapSide? KillerSide,
	string VictimName,
	string VictimSteamId64,
	MapSide? VictimSide,
	string? AssisterName,
	string Weapon,
	bool Headshot,
	bool Wallbang,
	bool ThroughSmoke,
	bool NoScope,
	bool AttackerBlind,
	bool IsOpening,
	bool IsTeamKill,
	bool? ByUs,
	string? KillerZone,
	string? VictimZone,
	float? KillerX,
	float? KillerY,
	float? VictimX,
	float? VictimY);

/// <summary>How an automatic insight should be read.</summary>
public enum InsightTone
{
	Neutral = 0,
	Positive = 1,
	Negative = 2
}

/// <summary>One automatic, rule-based observation about the match, with the evidence (<paramref name="Detail"/>) and
/// sample size it rests on.</summary>
public record MatchInsightDto(string Code, InsightTone Tone, string Title, string Detail, int SampleSize);
