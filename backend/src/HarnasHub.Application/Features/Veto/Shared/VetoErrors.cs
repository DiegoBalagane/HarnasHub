using ErrorOr;

namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>Domain errors for the Veto feature slice.</summary>
public static class VetoErrors
{
	/// <summary>The event the veto belongs to doesn't exist.</summary>
	public static Error EventNotFound => Error.NotFound(
		"Veto.EventNotFound",
		"Nie znaleziono wydarzenia.");
}
