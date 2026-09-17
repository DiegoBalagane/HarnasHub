using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Tactics.CreateTactic;

/// <summary>Creates a new, empty tactic for a map and side. Coach/Manager only.</summary>
public record CreateTacticCommand(
	MapName MapName,
	MapSide Side,
	string Name,
	EconomyType Economy,
	string? Note) : IRequest<ErrorOr<TacticDetailDto>>;
