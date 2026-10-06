using ErrorOr;
using HarnasHub.Application.Features.OpponentManagement.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentManagement.DeleteOpponent;

/// <summary>Deletes an opponent's scouting data (notes, FACEIT link, report, demo analyses); with <paramref name="IncludeHistory"/>
/// also its results and events, otherwise the opponent is hidden when any remain. Coach/Manager only — enforced at the endpoint.</summary>
public record DeleteOpponentCommand(string Name, bool IncludeHistory) : IRequest<ErrorOr<DeleteOpponentResultDto>>;
