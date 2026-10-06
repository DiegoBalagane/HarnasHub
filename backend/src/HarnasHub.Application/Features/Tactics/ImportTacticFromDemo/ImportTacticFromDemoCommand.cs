#region Usings

using ErrorOr;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Enums;
using MediatR;

#endregion

namespace HarnasHub.Application.Features.Tactics.ImportTacticFromDemo;

/// <summary>Saves grenades picked from a demo round as a new tactic (one point per landing spot, ordered by throw time),
/// optionally also adding them to the nade library. Coach/Manager only, enforced at the endpoint.</summary>
public record ImportTacticFromDemoCommand(
	MapName MapName,
	MapSide Side,
	string Name,
	EconomyType Economy,
	string? Note,
	IReadOnlyList<ImportedNadeInput> Grenades,
	bool AddToNadeLibrary) : IRequest<ErrorOr<TacticDetailDto>>;
