using ErrorOr;

namespace HarnasHub.Application.Features.MapStrategy.Shared;

/// <summary>Domain errors for the MapStrategy feature slice.</summary>
public static class MapStrategyErrors
{
	public static Error UserNotFound => Error.NotFound(
		"MapStrategy.UserNotFound",
		"Nie znaleziono zawodnika, któremu chcesz przypisać pozycję.");

	public static Error PositionNotFound => Error.NotFound(
		"MapStrategy.PositionNotFound",
		"Nie znaleziono pozycji na mapie.");
}
