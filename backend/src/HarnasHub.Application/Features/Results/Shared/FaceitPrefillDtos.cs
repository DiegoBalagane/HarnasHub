#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>One faction of a recognised FACEIT match: its name, the demo team it played as ("A"/"B", null when unknown),
/// its players and — when an opponent is already linked to this roster — that opponent's display name.</summary>
public record FaceitFactionPrefillDto(
	string Name,
	string? DemoTeam,
	IReadOnlyList<string> PlayerIds,
	IReadOnlyList<string> Nicknames,
	string? LinkedOpponentName);

/// <summary>Form prefill from the FACEIT match a demo's file name points at; every value is only a suggestion the coach can
/// edit. <paramref name="OurFactionIndex"/> is null when our roster couldn't be found in the room — the client then takes
/// the faction whose <see cref="FaceitFactionPrefillDto.DemoTeam"/> differs from the picked demo team as the opponent.</summary>
public record FaceitMatchPrefillDto(
	string MatchId,
	string? CompetitionName,
	MatchCategory Category,
	DateTime? PlayedAtUtc,
	string? MapName,
	int? OurFactionIndex,
	IReadOnlyList<FaceitFactionPrefillDto> Factions);
