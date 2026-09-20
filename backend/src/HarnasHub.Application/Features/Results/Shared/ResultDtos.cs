using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>A logged match/scrim/tournament result, with its tournament/league grouping already resolved for display.</summary>
public record MatchResultDto(
	Guid Id,
	string Opponent,
	int OurScore,
	int OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc,
	MatchCategory Category,
	Guid? TournamentId,
	string? TournamentName,
	Guid? LeagueId,
	string? LeagueName,
	string? LeagueSeason,
	LeagueType? LeagueType);
