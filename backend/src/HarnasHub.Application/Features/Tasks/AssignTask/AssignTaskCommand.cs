using ErrorOr;
using HarnasHub.Application.Features.Tasks.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.AssignTask;

/// <summary>Assigns a new task to a player, optionally attaching a review material. Coach/Manager only — enforced at the endpoint.</summary>
public record AssignTaskCommand(
	string Title,
	string? Description,
	Guid AssignedToUserId,
	DateTime? DueAtUtc,
	Guid? TrainingMaterialId) : IRequest<ErrorOr<TaskItemDto>>;
