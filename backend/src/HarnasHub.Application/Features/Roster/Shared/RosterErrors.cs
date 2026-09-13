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
}
