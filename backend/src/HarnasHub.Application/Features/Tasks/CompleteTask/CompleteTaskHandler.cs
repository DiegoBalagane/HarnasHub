using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.CompleteTask;

/// <summary>Handles <see cref="CompleteTaskCommand"/>.</summary>
public class CompleteTaskHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<CompleteTaskCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

		if (task is null)
		{
			return TaskErrors.TaskNotFound;
		}

		if (task.AssignedToUserId != currentUser.UserId)
		{
			return TaskErrors.NotYourTask;
		}

		task.Status = TaskItemStatus.Done;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
