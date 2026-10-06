#region Usings

using System.Linq.Expressions;
using HarnasHub.Core.Entities;

#endregion

namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>Single source of truth for turning an <see cref="Event"/> into an <see cref="EventDto"/>, both in-memory and inside EF queries.</summary>
public static class EventMappings
{
	#region Public Properties

	/// <summary>EF-translatable projection, for use in <c>.Select(...)</c> on an <see cref="Event"/> query.</summary>
	public static Expression<Func<Event, EventDto>> Projection { get; } = e => new EventDto(
		e.Id,
		e.Title,
		e.Type.ToString(),
		e.StartsAtUtc,
		e.EndsAtUtc,
		e.Location,
		e.Url,
		e.Notes,
		e.Opponent);

	#endregion

	#region Public Methods

	/// <summary>Maps an already-loaded event to its DTO.</summary>
	public static EventDto ToDto(Event calendarEvent) => CompiledProjection(calendarEvent);

	/// <summary>Trims an opponent name and turns blank input into null, so an empty form field never stores "".</summary>
	public static string? NormalizeOpponent(string? opponent) =>
		string.IsNullOrWhiteSpace(opponent) ? null : opponent.Trim();

	#endregion

	#region Private Fields

	private static readonly Func<Event, EventDto> CompiledProjection = Projection.Compile();

	#endregion
}
