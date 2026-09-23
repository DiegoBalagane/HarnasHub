using ErrorOr;
using HarnasHub.Application.Features.Tasks.Shared;
using MediatR;

namespace HarnasHub.Application.Features.Tasks.UpdateTask;

/// <summary>Edits a task's content (title/description/due date/material) in place — never touches its status or
/// assignee, those go through the review/reassignment flows instead. Coach/Manager only — enforced at the endpoint.</summary>
public record UpdateTaskCommand(
	Guid TaskId,
	string Title,
	string? Description,
	DateTime? DueAtUtc,
	Guid? TrainingMaterialId) : IRequest<ErrorOr<TaskItemDto>>;
