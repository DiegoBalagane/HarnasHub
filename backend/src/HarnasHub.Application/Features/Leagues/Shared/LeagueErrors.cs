using ErrorOr;

namespace HarnasHub.Application.Features.Leagues.Shared;

/// <summary>Domain errors for the Leagues feature slice.</summary>
public static class LeagueErrors
{
	public static Error LeagueNotFound => Error.NotFound(
		"Leagues.LeagueNotFound",
		"Nie znaleziono ligi.");
}
