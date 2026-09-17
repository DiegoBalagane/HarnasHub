using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Enums;
using MediatR;

namespace HarnasHub.Application.Features.Tactics.UpdateTactic;

/// <summary>Replaces a tactic's metadata and its entire radar layout in one save. Coach/Manager only.</summary>
public record UpdateTacticCommand(
	Guid TacticId,
	string Name,
	EconomyType Economy,
	string? Note,
	List<TacticPointInput> Points) : IRequest<ErrorOr<TacticDetailDto>>;
