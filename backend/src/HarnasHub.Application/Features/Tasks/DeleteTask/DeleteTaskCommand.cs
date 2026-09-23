using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.DeleteTask;

/// <summary>Coach/Manager permanently removes a task, e.g. one assigned by mistake.</summary>
public record DeleteTaskCommand(Guid TaskId) : IRequest<ErrorOr<Success>>;
