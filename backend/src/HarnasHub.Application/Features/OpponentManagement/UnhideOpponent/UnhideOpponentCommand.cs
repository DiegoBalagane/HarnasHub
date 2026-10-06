using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.OpponentManagement.UnhideOpponent;

/// <summary>Brings a hidden opponent back to the list and name suggestions. Coach/Manager only — enforced at the endpoint.</summary>
public record UnhideOpponentCommand(string Name) : IRequest<ErrorOr<Success>>;
