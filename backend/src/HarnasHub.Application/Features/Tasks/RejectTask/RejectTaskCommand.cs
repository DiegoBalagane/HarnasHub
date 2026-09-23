using ErrorOr;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.RejectTask;

/// <summary>Coach/Manager sends a player-submitted task back for rework.</summary>
public record RejectTaskCommand(Guid TaskId) : IRequest<ErrorOr<Success>>;
