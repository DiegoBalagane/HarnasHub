using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.SubmitTaskForReview;

/// <summary>Handles <see cref="SubmitTaskForReviewCommand"/>.</summary>
public class SubmitTaskForReviewHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<SubmitTaskForReviewCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(SubmitTaskForReviewCommand request, CancellationToken cancellationToken)
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

		if (task.Status is not (TaskItemStatus.Todo or TaskItemStatus.NeedsRework))
		{
			return TaskErrors.NotSubmittable;
		}

		task.Status = TaskItemStatus.PendingReview;
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
