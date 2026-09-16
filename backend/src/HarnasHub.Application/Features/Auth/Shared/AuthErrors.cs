using ErrorOr;

namespace HarnasHub.Application.Features.Auth.Shared;

/// <summary>Domain errors for the Auth feature slice.</summary>
public static class AuthErrors
{
	public static Error DiscordExchangeFailed => Error.Failure(
		"Auth.DiscordExchangeFailed",
		"Nie udało się zalogować przez Discorda — spróbuj ponownie, albo upewnij się, że jesteś na serwerze Discord drużyny.");

	public static Error UserNotFound => Error.NotFound(
		"Auth.UserNotFound",
		"Twoje konto nie zostało znalezione — zaloguj się ponownie.");
}
