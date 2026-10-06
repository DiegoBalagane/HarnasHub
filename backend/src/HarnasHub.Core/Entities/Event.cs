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
	/// <summary>Optional end time, for an event with a real duration (e.g. a training block) rather than a single moment.</summary>
	public DateTime? EndsAtUtc { get; set; }
	public string? Location { get; set; }
	public string? Url { get; set; }
	public string? Notes { get; set; }
	/// <summary>Optional opponent team name for a match/scrim/tournament game, matched case-insensitively against results and scouting notes.</summary>
	public string? Opponent { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime? ReminderSentAtUtc { get; set; }
	/// <summary>When the opponent-report briefing was posted to Discord for this match, so it is sent only once.</summary>
	public DateTime? BriefingSentAtUtc { get; set; }

	#endregion
}
