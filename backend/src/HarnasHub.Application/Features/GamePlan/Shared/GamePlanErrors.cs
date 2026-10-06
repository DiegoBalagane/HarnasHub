using ErrorOr;

namespace HarnasHub.Application.Features.GamePlan.Shared;

/// <summary>Domain errors for the GamePlan feature slice.</summary>
public static class GamePlanErrors
{
	/// <summary>The event the plan belongs to doesn't exist.</summary>
	public static Error EventNotFound => Error.NotFound(
		"GamePlan.EventNotFound",
		"Nie znaleziono wydarzenia.");

	/// <summary>Some of the attached tactics or boards don't exist (e.g. deleted meanwhile).</summary>
	public static Error UnknownItems => Error.Validation(
		"GamePlan.UnknownItems",
		"Część wybranych taktyk lub tablic już nie istnieje — odśwież stronę i wybierz ponownie.");
}
