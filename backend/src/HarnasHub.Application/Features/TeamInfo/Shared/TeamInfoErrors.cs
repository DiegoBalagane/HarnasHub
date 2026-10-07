using ErrorOr;

namespace HarnasHub.Application.Features.TeamInfo.Shared;

/// <summary>Domain errors for the TeamInfo feature slice.</summary>
public static class TeamInfoErrors
{
	/// <summary>The info entry doesn't exist (or was already deleted).</summary>
	public static Error EntryNotFound => Error.NotFound(
		"TeamInfo.EntryNotFound",
		"Nie znaleziono wpisu.");
}
