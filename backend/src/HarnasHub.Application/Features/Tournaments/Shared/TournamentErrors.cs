using ErrorOr;

namespace HarnasHub.Application.Features.Tournaments.Shared;

/// <summary>Domain errors for the Tournaments feature slice.</summary>
public static class TournamentErrors
{
	public static Error TournamentNotFound => Error.NotFound(
		"Tournaments.TournamentNotFound",
		"Nie znaleziono turnieju.");
}
