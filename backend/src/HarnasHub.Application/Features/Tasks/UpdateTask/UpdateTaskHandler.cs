using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.UpdateTask;

/// <summary>Handles <see cref="UpdateTaskCommand"/>.</summary>
public class UpdateTaskHandler(IApplicationDbContext dbContext, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateTaskCommand, ErrorOr<TaskItemDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TaskItemDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
	{
		var task = await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == request.TaskId, cancellationToken);

		if (task is null)
		{
			return TaskErrors.TaskNotFound;
		}

		TrainingMaterial? material = null;

		if (request.TrainingMaterialId is not null)
		{
			material = await dbContext.TrainingMaterials
				.FirstOrDefaultAsync(m => m.Id == request.TrainingMaterialId, cancellationToken);

			if (material is null)
			{
				return TaskErrors.MaterialNotFound;
			}
		}

		task.Title = request.Title;
		task.Description = request.Description;
		task.DueAtUtc = request.DueAtUtc;
		task.TrainingMaterialId = material?.Id;

		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new TaskItemDto(
			task.Id, task.Title, task.Description, task.Status.ToString(), task.DueAtUtc, task.CreatedAtUtc,
			material?.Id, material?.Title, material?.Url);
	}

	#endregion
}
