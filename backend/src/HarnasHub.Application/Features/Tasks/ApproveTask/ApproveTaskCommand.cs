using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.ApproveTask;

/// <summary>Coach/Manager confirms a player-submitted task as done.</summary>
public record ApproveTaskCommand(Guid TaskId) : IRequest<ErrorOr<Success>>;
