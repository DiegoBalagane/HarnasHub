using ErrorOr;
using HarnasHub.Application.Features.Tasks.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.GetMyTasks;

/// <summary>Returns every task assigned to the current user, open tasks first.</summary>
public record GetMyTasksQuery : IRequest<ErrorOr<List<TaskItemDto>>>;
