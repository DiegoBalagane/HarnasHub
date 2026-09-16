using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.MapStrategy.RemovePlayerPosition;

/// <summary>Removes one player's starting spot from the map setup.</summary>
public record RemovePlayerPositionCommand(Guid PositionId) : IRequest<ErrorOr<Success>>;
