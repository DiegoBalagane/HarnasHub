using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.ApproveTask;

/// <summary>Handles <see cref="ApproveTaskCommand"/>.</summary>
public class ApproveTaskHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<ApproveTaskCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(ApproveTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

		if (task is null)
		{
			return TaskErrors.TaskNotFound;
		}

		if (task.Status != TaskItemStatus.PendingReview)
		{
			return TaskErrors.NotPendingReview;
		}

		task.Status = TaskItemStatus.Done;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
