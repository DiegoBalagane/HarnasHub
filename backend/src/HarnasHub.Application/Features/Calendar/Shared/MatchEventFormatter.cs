#region Usings

using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>Pure Polish texts and routing for calendar notifications: match events go to the match schedule channel, everything else to announcements.</summary>
public static class MatchEventFormatter
{
	#region Public Methods

	/// <summary>The channel an event's notifications (create message, reminder) belong to.</summary>
	public static DiscordChannel ChannelFor(EventType type) =>
		type == EventType.Match ? DiscordChannel.MatchSchedule : DiscordChannel.Announcements;

	/// <summary>The Polish label of an event type, e.g. "Mecz".</summary>
	public static string TypeLabel(EventType type) => type switch
	{
		EventType.Training => "Trening",
		EventType.PickupGame => "Gra luźna",
		EventType.Match => "Mecz",
		EventType.Tournament => "Turniej",
		EventType.Scrim => "Sparing",
		_ => type.ToString()
	};

	/// <summary>Text for a freshly created event of any type.</summary>
	public static string Created(Event calendarEvent) =>
		$"📅 Nowe wydarzenie: **{calendarEvent.Title}** ({TypeLabel(calendarEvent.Type)}){OpponentPart(calendarEvent)} — {When(calendarEvent)}";

	/// <summary>Text for an edited match event.</summary>
	public static string Updated(Event calendarEvent) =>
		$"✏️ Zmiana w meczu: **{calendarEvent.Title}**{OpponentPart(calendarEvent)} — {When(calendarEvent)}{LocationPart(calendarEvent)}";

	/// <summary>Text for a deleted match event.</summary>
	public static string Deleted(Event calendarEvent) =>
		$"🗑️ Usunięto mecz z kalendarza: **{calendarEvent.Title}**{OpponentPart(calendarEvent)} — {When(calendarEvent)}";

	/// <summary>Reminder text: "⏰ … zaczyna się za N min".</summary>
	public static string Reminder(Event calendarEvent, int minutesUntil) =>
		$"⏰ **{calendarEvent.Title}** zaczyna się za {minutesUntil} min{LocationPart(calendarEvent)}";

	/// <summary>True when an edit touched type, title, time, place or opponent — notes/link-only edits stay silent.</summary>
	public static bool IsSignificantChange(Event before, Event after) =>
		before.Type != after.Type
		|| before.Title != after.Title
		|| before.StartsAtUtc != after.StartsAtUtc
		|| before.EndsAtUtc != after.EndsAtUtc
		|| before.Location != after.Location
		|| before.Opponent != after.Opponent;

	#endregion

	#region Private Methods

	private static string When(Event e) => $"{e.StartsAtUtc:dd.MM HH:mm}";

	private static string OpponentPart(Event e) => string.IsNullOrWhiteSpace(e.Opponent) ? "" : $" vs {e.Opponent}";

	private static string LocationPart(Event e) => string.IsNullOrWhiteSpace(e.Location) ? "" : $" @ {e.Location}";

	#endregion
}
