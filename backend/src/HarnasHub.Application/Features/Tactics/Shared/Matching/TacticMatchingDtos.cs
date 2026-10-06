#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared.Matching;

/// <summary>Per-round tactic matches of one match. <paramref name="MapCalibrated"/> false (Dust2/Inferno/Nuke today) or
/// <paramref name="HasPositions"/> false (timeline parsed before positions were recorded) explain an empty or grenade-only result.</summary>
public record MatchTacticMatchesDto(
	bool MapCalibrated,
	bool HasPositions,
	bool OurTeamResolved,
	List<RoundTacticMatchDto> Rounds);

/// <summary>The tactic one round was matched to and how well (0–100).</summary>
public record RoundTacticMatchDto(int RoundNumber, MapSide Side, Guid TacticId, string TacticName, int ScorePercent);

/// <summary>How each tactic of a map fared over the newest analysed matches on it.</summary>
public record TacticEffectivenessReportDto(
	MapName Map,
	bool MapCalibrated,
	int MatchesAnalyzed,
	int MatchesSkipped,
	int RoundsAnalyzed,
	int RoundsMatched,
	List<TacticEffectivenessDto> Tactics);

/// <summary>One tactic's record: rounds matched to it, how many of them we won and in how many matches it appeared.</summary>
public record TacticEffectivenessDto(Guid TacticId, string Name, MapSide Side, int RoundsPlayed, int RoundsWon, int Matches);
