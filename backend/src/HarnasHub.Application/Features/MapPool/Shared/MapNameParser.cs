using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.MapPool.Shared;

/// <summary>Maps the free-text <c>MatchResult.MapName</c> (manual entry or the demo parser's enum name) onto the pool enum.</summary>
public static class MapNameParser
{
	#region Public Methods

	/// <summary>The pool map for <paramref name="mapName"/>, ignoring case and surrounding spaces; null for blanks and maps outside the pool.</summary>
	public static MapName? Parse(string? mapName) =>
		!string.IsNullOrWhiteSpace(mapName) && Enum.TryParse<MapName>(mapName.Trim(), ignoreCase: true, out var map) && Enum.IsDefined(map)
			? map
			: null;

	#endregion
}
