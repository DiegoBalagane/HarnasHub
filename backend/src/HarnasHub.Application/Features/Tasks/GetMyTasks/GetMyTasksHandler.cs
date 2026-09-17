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
			.GroupJoin(dbContext.TrainingMaterials, t => t.TrainingMaterialId, m => m.Id, (t, materials) => new { t, materials })
			.SelectMany(x => x.materials.DefaultIfEmpty(), (x, material) => new { x.t, material })
			.OrderBy(x => x.t.Status == TaskItemStatus.Done)
			.ThenBy(x => x.t.DueAtUtc)
			.Select(x => new TaskItemDto(
				x.t.Id,
				x.t.Title,
				x.t.Description,
				x.t.Status.ToString(),
				x.t.DueAtUtc,
				x.t.CreatedAtUtc,
				x.t.TrainingMaterialId,
				x.material == null ? null : x.material.Title,
				x.material == null ? null : x.material.Url))
			.ToListAsync(cancellationToken);
	}

	#endregion
}
