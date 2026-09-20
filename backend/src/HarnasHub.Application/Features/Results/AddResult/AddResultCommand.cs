using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Logs a scrim/match/tournament result. Coach/Manager only — enforced at the endpoint.
/// <paramref name="TournamentId"/> is set only for <see cref="MatchCategory.Tournament"/>, <paramref name="LeagueId"/> only for <see cref="MatchCategory.League"/>.</summary>
public record AddResultCommand(
	string Opponent,
	int OurScore,
	int OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc,
	MatchCategory Category,
	Guid? TournamentId,
	Guid? LeagueId) : IRequest<ErrorOr<MatchResultDto>>;
