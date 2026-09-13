using ErrorOr;

namespace HarnasHub.Application.Features.Stats.Shared;

/// <summary>Domain errors for the Stats feature slice.</summary>
public static class StatsErrors
{
    public static Error MatchNotFound => Error.Validation(
        "Stats.MatchNotFound",
        "Nie znaleziono meczu.");

    public static Error PlayerNotFound => Error.Validation(
        "Stats.PlayerNotFound",
        "Nie znaleziono zawodnika.");

    public static Error StatAlreadyExists => Error.Conflict(
        "Stats.StatAlreadyExists",
        "Ten zawodnik ma już wpisane statystyki dla tego meczu.");
}
