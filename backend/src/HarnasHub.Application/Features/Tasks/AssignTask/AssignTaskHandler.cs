using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Application.Features.Tasks.AssignTask;

/// <summary>Handles <see cref="AssignTaskCommand"/> by persisting the new task.</summary>
public class AssignTaskHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    : IRequestHandler<AssignTaskCommand, ErrorOr<TaskItemDto>>
{
    #region Public Methods

    public async Task<ErrorOr<TaskItemDto>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
    {
        var assigneeExists = await dbContext.Users.AnyAsync(u => u.Id == request.AssignedToUserId, cancellationToken);

        if (!assigneeExists)
        {
            return TaskErrors.AssigneeNotFound;
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            AssignedToUserId = request.AssignedToUserId,
            AssignedByUserId = currentUser.UserId,
            Status = TaskItemStatus.Todo,
            DueAtUtc = request.DueAtUtc,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Tasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TaskItemDto(task.Id, task.Title, task.Description, task.Status.ToString(), task.DueAtUtc, task.CreatedAtUtc);
    }

    #endregion
}
