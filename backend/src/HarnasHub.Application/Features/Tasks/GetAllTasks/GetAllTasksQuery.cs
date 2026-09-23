using ErrorOr;
using HarnasHub.Application.Features.Tasks.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.GetAllTasks;

/// <summary>Coach/Manager overview of every player's assigned tasks.</summary>
public record GetAllTasksQuery : IRequest<ErrorOr<List<TaskItemWithAssigneeDto>>>;
