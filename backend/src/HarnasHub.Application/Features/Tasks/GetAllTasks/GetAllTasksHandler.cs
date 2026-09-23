using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tasks.Shared;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Tasks.GetAllTasks;

/// <summary>Handles <see cref="GetAllTasksQuery"/>.</summary>
public class GetAllTasksHandler(IApplicationDbContext dbContext) : IRequestHandler<GetAllTasksQuery, ErrorOr<List<TaskItemWithAssigneeDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<TaskItemWithAssigneeDto>>> Handle(GetAllTasksQuery request, CancellationToken cancellationToken)
	{
		return await dbContext.Tasks
			.Join(dbContext.Users, t => t.AssignedToUserId, u => u.Id, (t, u) => new { t, u })
			.GroupJoin(dbContext.TrainingMaterials, x => x.t.TrainingMaterialId, m => m.Id, (x, materials) => new { x.t, x.u, materials })
			.SelectMany(x => x.materials.DefaultIfEmpty(), (x, material) => new { x.t, x.u, material })
			.OrderBy(x => x.u.InGameNickname ?? x.u.DisplayName)
			.ThenBy(x => x.t.Status == TaskItemStatus.Done)
			.ThenBy(x => x.t.DueAtUtc)
			.Select(x => new TaskItemWithAssigneeDto(
				x.t.Id,
				x.t.Title,
				x.t.Description,
				x.t.Status.ToString(),
				x.t.DueAtUtc,
				x.t.CreatedAtUtc,
				x.t.TrainingMaterialId,
				x.material == null ? null : x.material.Title,
				x.material == null ? null : x.material.Url,
				x.u.Id,
				x.u.InGameNickname ?? x.u.DisplayName))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
