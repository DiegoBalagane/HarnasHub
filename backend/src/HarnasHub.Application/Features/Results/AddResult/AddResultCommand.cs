using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Logs a scrim/match/tournament result. Coach/Manager only — enforced at the endpoint.
/// <paramref name="TournamentId"/> is set only for <see cref="MatchCategory.Tournament"/>, <paramref name="LeagueId"/> only for <see cref="MatchCategory.League"/>.
/// <paramref name="DemoStream"/> is an optional uploaded .dem read in memory to derive the score and map — never stored anywhere;
/// <paramref name="OurScore"/>/<paramref name="OpponentScore"/>/<paramref name="MapName"/> are the manual fallback when there's no demo (or none of its rounds can be attributed to our roster).</summary>
public record AddResultCommand(
	string Opponent,
	int? OurScore,
	int? OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc,
	MatchCategory Category,
	Guid? TournamentId,
	Guid? LeagueId,
	Stream? DemoStream = null) : IRequest<ErrorOr<MatchResultDto>>;
