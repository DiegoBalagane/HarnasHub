using ErrorOr;
using HarnasHub.Application.Features.OpponentManagement.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentManagement.RenameOpponent;

/// <summary>Renames the opponent <paramref name="From"/> to <paramref name="To"/>, merging it into the target when that name already exists.
/// Coach/Manager only — enforced at the endpoint.</summary>
public record RenameOpponentCommand(string From, string To) : IRequest<ErrorOr<RenameOpponentResultDto>>;
