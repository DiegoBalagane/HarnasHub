using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.DeleteTask;

/// <summary>Handles <see cref="DeleteTaskCommand"/>.</summary>
public class DeleteTaskHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteTaskCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

		if (task is null)
		{
			return TaskErrors.TaskNotFound;
		}

		dbContext.Tasks.Remove(task);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return Result.Success;
	}

	#endregion
}
