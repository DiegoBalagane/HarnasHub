using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.OpponentManagement.HideOpponent;

/// <summary>Hides an opponent from the list and name suggestions without touching its history. Coach/Manager only — enforced at the endpoint.</summary>
public record HideOpponentCommand(string Name) : IRequest<ErrorOr<Success>>;
