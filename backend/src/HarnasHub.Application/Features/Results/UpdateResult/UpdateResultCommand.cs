using ErrorOr;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Results.UpdateResult;

/// <summary>Edits a logged result's metadata (opponent/score/map/date/category/etc.) — never touches its imported
/// player stat lines, since those came from a demo already parsed and shouldn't silently change here.
/// Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateResultCommand(
	Guid MatchResultId,
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
