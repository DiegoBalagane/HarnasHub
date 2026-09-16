namespace HarnasHub.Core.Options;

/// <summary>Configuration for the upcoming-event Discord reminder background check.</summary>
public class ReminderSettings
{
	public const string SectionName = "Reminders";

	/// <summary>How far ahead of an event's start time a reminder is sent.</summary>
	public int LookaheadMinutes { get; set; } = 60;

	/// <summary>How often the background check runs.</summary>
	public int CheckIntervalSeconds { get; set; } = 60;
}
