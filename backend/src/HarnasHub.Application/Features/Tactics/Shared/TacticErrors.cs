using ErrorOr;

namespace HarnasHub.Application.Features.Tactics.Shared;

/// <summary>Domain errors for the Tactics feature slice.</summary>
public static class TacticErrors
{
	public static Error NotFound => Error.NotFound(
		"Tactics.NotFound",
		"Nie znaleziono taktyki.");

	/// <summary>The demo's map is outside the pool or has no radar calibration, so grenade positions can't be placed on the radar.</summary>
	public static Error DemoMapNotSupported => Error.Validation(
		"Tactics.DemoMapNotSupported",
		"Ta demka jest z mapy, której radar nie jest jeszcze skalibrowany — import granatów jest niemożliwy.");
}
