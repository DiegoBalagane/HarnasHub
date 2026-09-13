using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.CompleteTask;

/// <summary>Marks a task assigned to the current user as done.</summary>
public record CompleteTaskCommand(Guid TaskId) : IRequest<ErrorOr<Success>>;
