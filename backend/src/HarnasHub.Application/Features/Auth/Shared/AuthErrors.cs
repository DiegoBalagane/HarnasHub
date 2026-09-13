using ErrorOr;

namespace HarnasHub.Application.Features.Auth.Shared;

/// <summary>Domain errors for the Auth feature slice.</summary>
public static class AuthErrors
{
    public static Error EmailAlreadyRegistered => Error.Conflict(
        "Auth.EmailAlreadyRegistered",
        "Konto z tym adresem e-mail już istnieje.");

    public static Error InvalidCredentials => Error.Unauthorized(
        "Auth.InvalidCredentials",
        "Nieprawidłowy e-mail lub hasło.");
}
