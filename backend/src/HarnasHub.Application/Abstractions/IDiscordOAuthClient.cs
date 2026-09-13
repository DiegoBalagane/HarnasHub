namespace HarnasHub.Application.Abstractions;

/// <summary>The Discord profile fields we need after a successful OAuth exchange.</summary>
public record DiscordProfile(string DiscordId, string Username, string? AvatarUrl);

/// <summary>Exchanges an OAuth2 authorization code for the caller's Discord profile.</summary>
public interface IDiscordOAuthClient
{
    Task<DiscordProfile?> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
}
