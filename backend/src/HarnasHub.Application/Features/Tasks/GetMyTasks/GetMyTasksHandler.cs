using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.GetMyTasks;

/// <summary>Handles <see cref="GetMyTasksQuery"/>.</summary>
public class GetMyTasksHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser)
    : IRequestHandler<GetMyTasksQuery, ErrorOr<List<TaskItemDto>>>
{
    #region Public Methods

    public async Task<ErrorOr<List<TaskItemDto>>> Handle(GetMyTasksQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;

        return await dbContext.Tasks
            .Where(t => t.AssignedToUserId == userId)
            .OrderBy(t => t.Status == TaskItemStatus.Done)
            .ThenBy(t => t.DueAtUtc)
            .Select(t => new TaskItemDto(t.Id, t.Title, t.Description, t.Status.ToString(), t.DueAtUtc, t.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    #endregion
}
