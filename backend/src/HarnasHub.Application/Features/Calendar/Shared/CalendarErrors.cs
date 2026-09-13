using ErrorOr;

namespace HarnasHub.Application.Features.Calendar.Shared;

/// <summary>Domain errors for the Calendar feature slice.</summary>
public static class CalendarErrors
{
    public static Error EventNotFound => Error.NotFound(
        "Calendar.EventNotFound",
        "Nie znaleziono wydarzenia.");
}
