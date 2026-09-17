using ErrorOr;

namespace HarnasHub.Application.Features.Tactics.Shared;

/// <summary>Domain errors for the Tactics feature slice.</summary>
public static class TacticErrors
{
	public static Error NotFound => Error.NotFound(
		"Tactics.NotFound",
		"Nie znaleziono taktyki.");
}
