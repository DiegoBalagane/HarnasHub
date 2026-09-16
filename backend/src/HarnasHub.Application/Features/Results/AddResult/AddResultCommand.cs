using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Results.AddResult;

/// <summary>Logs a scrim/match/tournament result. Coach/Manager only — enforced at the endpoint.</summary>
public record AddResultCommand(
	string Opponent,
	int OurScore,
	int OpponentScore,
	string? MapName,
	string? DemoUrl,
	string? Notes,
	DateTime PlayedAtUtc) : IRequest<ErrorOr<MatchResultDto>>;
