using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A calendar entry — training, pickup game, match, or tournament.</summary>
public class Event
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public EventType Type { get; set; }
	public DateTime StartsAtUtc { get; set; }
	public string? Location { get; set; }
	public string? Notes { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? ReminderSentAtUtc { get; set; }

	#endregion
}
