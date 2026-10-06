using ErrorOr;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Domain errors for the OpponentReport feature slice.</summary>
public static class OpponentReportErrors
{
	/// <summary>No FACEIT API key is configured on the server.</summary>
	public static Error FaceitNotConfigured => Error.Failure(
		"OpponentReport.FaceitNotConfigured",
		"Integracja z FACEIT nie jest skonfigurowana — administrator musi ustawić klucz Faceit:ApiKey.");

	/// <summary>FACEIT was unreachable or kept failing after retries.</summary>
	public static Error FaceitUnavailable => Error.Failure(
		"OpponentReport.FaceitUnavailable",
		"Nie udało się pobrać danych z FACEIT. Spróbuj ponownie później.");

	/// <summary>The opponent has no FACEIT roster linked yet.</summary>
	public static Error NotLinked => Error.NotFound(
		"OpponentReport.NotLinked",
		"Przeciwnik nie jest powiązany z FACEIT — wklej link do drużyny, pokoju meczowego albo listę nicków.");

	/// <summary>A manual refresh came in before the cooldown expired.</summary>
	public static Error RefreshTooSoon(int minutesLeft) => Error.Conflict(
		"OpponentReport.RefreshTooSoon",
		$"Dane odświeżano przed chwilą — spróbuj ponownie za {minutesLeft} min.");

	/// <summary>The pasted text is neither a FACEIT team/room link nor a list of nicknames.</summary>
	public static Error InvalidSource => Error.Validation(
		"OpponentReport.InvalidSource",
		"Wklej link do drużyny FACEIT, link do pokoju meczowego albo listę nicków.");

	/// <summary>The FACEIT team from the link doesn't exist.</summary>
	public static Error TeamNotFound => Error.NotFound(
		"OpponentReport.TeamNotFound",
		"Nie znaleziono drużyny FACEIT z tego linku.");

	/// <summary>The FACEIT match room from the link doesn't exist.</summary>
	public static Error MatchNotFound => Error.NotFound(
		"OpponentReport.MatchNotFound",
		"Nie znaleziono meczu FACEIT z tego linku.");

	/// <summary>Some of the pasted nicknames don't exist on FACEIT.</summary>
	public static Error PlayersNotFound(IEnumerable<string> nicknames) => Error.NotFound(
		"OpponentReport.PlayersNotFound",
		$"Nie znaleziono graczy FACEIT: {string.Join(", ", nicknames)}.");

	/// <summary>Neither side of the match room could be identified as the opponent.</summary>
	public static Error AmbiguousMatch => Error.Validation(
		"OpponentReport.AmbiguousMatch",
		"Nie da się ustalić, która drużyna w tym meczu to przeciwnik — wklej link do drużyny albo listę nicków.");

	/// <summary>The resolved roster has no players.</summary>
	public static Error EmptyRoster => Error.Validation(
		"OpponentReport.EmptyRoster",
		"Nie znaleziono żadnych graczy przeciwnika na FACEIT.");
}
