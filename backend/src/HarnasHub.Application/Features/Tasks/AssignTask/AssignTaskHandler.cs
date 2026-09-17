using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Application.Features.Tasks.AssignTask;

/// <summary>Handles <see cref="AssignTaskCommand"/> by persisting the new task and notifying the team.</summary>
public class AssignTaskHandler(
	IApplicationDbContext dbContext,
	ICurrentUserService currentUser,
	IDiscordNotifier discordNotifier,
	IRealtimeNotifier realtimeNotifier) : IRequestHandler<AssignTaskCommand, ErrorOr<TaskItemDto>>
{
	#region Public Methods

	public async Task<ErrorOr<TaskItemDto>> Handle(AssignTaskCommand request, CancellationToken cancellationToken)
	{
		var assignee = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.AssignedToUserId, cancellationToken);

		if (assignee is null)
		{
			return TaskErrors.AssigneeNotFound;
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

		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = request.Title,
			Description = request.Description,
			AssignedToUserId = request.AssignedToUserId,
			AssignedByUserId = currentUser.UserId,
			Status = TaskItemStatus.Todo,
			DueAtUtc = request.DueAtUtc,
			CreatedAtUtc = DateTime.UtcNow,
			TrainingMaterialId = material?.Id
		};

		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(cancellationToken);

		await discordNotifier.SendAsync($"📋 Nowe zadanie dla **{assignee.DisplayName}**: {task.Title}", cancellationToken);
		await realtimeNotifier.NotifyAsync("tasks", cancellationToken);
		await realtimeNotifier.NotifyAsync("dashboard", cancellationToken);

		return new TaskItemDto(
			task.Id, task.Title, task.Description, task.Status.ToString(), task.DueAtUtc, task.CreatedAtUtc,
			material?.Id, material?.Title, material?.Url);
	}

	#endregion
}
