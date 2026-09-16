using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A coach/manager-assigned action item for one player.</summary>
public class TaskItem
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid AssignedToUserId { get; set; }
	public Guid AssignedByUserId { get; set; }
	public TaskItemStatus Status { get; set; }
	public DateTime? DueAtUtc { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
