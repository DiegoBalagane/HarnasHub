namespace HarnasHub.Application.Features.Tasks.Shared;

/// <summary>A task assigned to a player, with its optional review material already joined in.</summary>
public record TaskItemDto(
	Guid Id,
	string Title,
	string? Description,
	string Status,
	DateTime? DueAtUtc,
	DateTime CreatedAtUtc,
	Guid? TrainingMaterialId,
	string? TrainingMaterialTitle,
	string? TrainingMaterialUrl);

/// <summary>A task with its assignee's name attached, for the coach/manager's all-players overview.</summary>
public record TaskItemWithAssigneeDto(
	Guid Id,
	string Title,
	string? Description,
	string Status,
	DateTime? DueAtUtc,
	DateTime CreatedAtUtc,
	Guid? TrainingMaterialId,
	string? TrainingMaterialTitle,
	string? TrainingMaterialUrl,
	Guid AssignedToUserId,
	string AssignedToDisplayName);
