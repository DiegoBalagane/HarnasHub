using ErrorOr;

namespace HarnasHub.Application.Features.Nades.Shared;

/// <summary>Domain errors for the Nades feature slice.</summary>
public static class NadeErrors
{
	public static Error NotFound => Error.NotFound(
		"Nades.NotFound",
		"Nie znaleziono granatu.");

	public static Error NotYourEntry => Error.Forbidden(
		"Nades.NotYourEntry",
		"Możesz usuwać tylko własne wpisy (chyba że jesteś coachem/managerem).");
}
