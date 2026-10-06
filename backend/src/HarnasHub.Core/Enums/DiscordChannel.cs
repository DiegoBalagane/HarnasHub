namespace HarnasHub.Core.Enums;

/// <summary>The team's Discord channels a notification can be routed to; each has its own webhook.</summary>
public enum DiscordChannel
{
	/// <summary>New non-match events, assigned tasks and regular event reminders.</summary>
	Announcements,

	/// <summary>Match events (created/updated/deleted), match reminders and saved results.</summary>
	MatchSchedule,

	/// <summary>Short digest after a demo was analysed and attached to a match result.</summary>
	DemoReview,

	/// <summary>Pre-match briefing and digests of analysed opponent demos.</summary>
	OpponentScouting
}
