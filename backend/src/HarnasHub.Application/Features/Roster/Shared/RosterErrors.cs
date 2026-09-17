using ErrorOr;

namespace HarnasHub.Application.Features.Roster.Shared;

/// <summary>Domain errors for the Roster feature slice.</summary>
public static class RosterErrors
{
	public static Error UserNotFound => Error.NotFound(
		"Roster.UserNotFound",
		"Nie znaleziono zawodnika.");

	public static Error CannotChangeOwnRole => Error.Validation(
		"Roster.CannotChangeOwnRole",
		"Nie możesz zmienić własnej roli — poproś innego managera.");

	public static Error MainRosterFull => Error.Validation(
		"Roster.MainRosterFull",
		"Główny skład jest już pełny (5 zawodników) — najpierw przenieś kogoś na ławkę.");

	public static Error PinColorRequiresMainRoster => Error.Validation(
		"Roster.PinColorRequiresMainRoster",
		"Kolor pinezki mogą wybrać tylko zawodnicy głównego składu.");

	public static Error SecondaryRoleMatchesPrimary => Error.Validation(
		"Roster.SecondaryRoleMatchesPrimary",
		"Rola dodatkowa nie może być taka sama jak rola główna zawodnika.");

	public static Error GuestCannotBeCoach => Error.Validation(
		"Roster.GuestCannotBeCoach",
		"Najpierw nadaj Gościowi poziom uprawnień (Zawodnik lub Zarządca), zanim oznaczysz go jako Trenera.");
}
