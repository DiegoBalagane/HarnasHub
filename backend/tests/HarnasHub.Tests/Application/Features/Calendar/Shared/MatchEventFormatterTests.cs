using HarnasHub.Application.Features.Calendar.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Calendar.Shared;

public class MatchEventFormatterTests
{
	#region Public Methods

	[Theory]
	[InlineData(EventType.Match, DiscordChannel.MatchSchedule)]
	[InlineData(EventType.Training, DiscordChannel.Announcements)]
	[InlineData(EventType.Scrim, DiscordChannel.Announcements)]
	[InlineData(EventType.Tournament, DiscordChannel.Announcements)]
	[InlineData(EventType.PickupGame, DiscordChannel.Announcements)]
	public void ChannelFor_routes_only_matches_to_the_match_schedule(EventType type, DiscordChannel expected)
	{
		Assert.Equal(expected, MatchEventFormatter.ChannelFor(type));
	}

	[Fact]
	public void Created_includes_title_type_opponent_and_time()
	{
		var text = MatchEventFormatter.Created(Make(opponent: "Team X"));

		Assert.Equal("📅 Nowe wydarzenie: **Liga R3** (Mecz) vs Team X — 20.09 20:00", text);
	}

	[Fact]
	public void Updated_and_deleted_mention_the_match()
	{
		var calendarEvent = Make(opponent: "Team X", location: "Discord");

		Assert.Equal("✏️ Zmiana w meczu: **Liga R3** vs Team X — 20.09 20:00 @ Discord", MatchEventFormatter.Updated(calendarEvent));
		Assert.Equal("🗑️ Usunięto mecz z kalendarza: **Liga R3** vs Team X — 20.09 20:00", MatchEventFormatter.Deleted(calendarEvent));
	}

	[Fact]
	public void Reminder_appends_the_location_when_known()
	{
		Assert.Equal("⏰ **Liga R3** zaczyna się za 30 min", MatchEventFormatter.Reminder(Make(), 30));
		Assert.Equal("⏰ **Liga R3** zaczyna się za 30 min @ Discord", MatchEventFormatter.Reminder(Make(location: "Discord"), 30));
	}

	[Fact]
	public void IsSignificantChange_ignores_notes_and_url_only_edits()
	{
		var before = Make();
		var notesOnly = Make();
		notesOnly.Notes = "nowa notatka";
		notesOnly.Url = "https://x";
		var moved = Make();
		moved.StartsAtUtc = moved.StartsAtUtc.AddHours(1);

		Assert.False(MatchEventFormatter.IsSignificantChange(before, notesOnly));
		Assert.True(MatchEventFormatter.IsSignificantChange(before, moved));
	}

	#endregion

	#region Private Methods

	private static Event Make(string? opponent = null, string? location = null) => new()
	{
		Title = "Liga R3",
		Type = EventType.Match,
		StartsAtUtc = new DateTime(2026, 9, 20, 18, 0, 0, DateTimeKind.Utc),
		Opponent = opponent,
		Location = location
	};

	#endregion
}
