using ErrorOr;

namespace HarnasHub.Application.Features.OpponentManagement.Shared;

/// <summary>Domain errors for deleting, hiding and renaming opponents.</summary>
public static class OpponentManagementErrors
{
	/// <summary>Nothing is stored under that opponent name.</summary>
	public static Error NotFound => Error.NotFound(
		"OpponentManagement.NotFound",
		"Nie znaleziono takiego przeciwnika.");
}
