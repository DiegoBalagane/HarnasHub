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

	/// <summary>Optional review material attached to this task — a loose reference without a DB-level FK, same pattern as <c>TacticPoint.NadeEntryId</c>.</summary>
	public Guid? TrainingMaterialId { get; set; }

	#endregion
}
