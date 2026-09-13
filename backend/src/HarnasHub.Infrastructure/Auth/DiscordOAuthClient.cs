using System.Net.Http.Json;
using System.Text.Json.Serialization;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure.Auth;

/// <summary>Exchanges a Discord OAuth2 authorization code for the caller's profile via Discord's own API.</summary>
public class DiscordOAuthClient(HttpClient httpClient, IOptions<DiscordOAuthSettings> settings, ILogger<DiscordOAuthClient> logger)
    : IDiscordOAuthClient
{
    #region Public Methods

    public async Task<DiscordProfile?> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var options = settings.Value;

        var tokenResponse = await httpClient.PostAsync(
            "https://discord.com/api/oauth2/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = options.RedirectUri
            }),
            cancellationToken);

        if (!tokenResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("Wymiana kodu Discord OAuth nie powiodła się: {StatusCode}", tokenResponse.StatusCode);
            return null;
        }

        var token = await tokenResponse.Content.ReadFromJsonAsync<DiscordTokenResponse>(cancellationToken: cancellationToken);

        if (token is null)
        {
            return null;
        }

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
        userRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.AccessToken);

        var userResponse = await httpClient.SendAsync(userRequest, cancellationToken);

        if (!userResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("Pobranie profilu Discord nie powiodło się: {StatusCode}", userResponse.StatusCode);
            return null;
        }

        var discordUser = await userResponse.Content.ReadFromJsonAsync<DiscordUserResponse>(cancellationToken: cancellationToken);

        if (discordUser is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(options.RequiredGuildId))
        {
            var isMember = await IsMemberOfRequiredGuildAsync(token.AccessToken, options.RequiredGuildId, cancellationToken);

            if (!isMember)
            {
                logger.LogWarning("Odrzucono logowanie Discord {DiscordId} — brak członkostwa w wymaganym serwerze", discordUser.Id);
                return null;
            }
        }

        var displayName = string.IsNullOrWhiteSpace(discordUser.GlobalName) ? discordUser.Username : discordUser.GlobalName;
        var avatarUrl = discordUser.Avatar is null
            ? null
            : $"https://cdn.discordapp.com/avatars/{discordUser.Id}/{discordUser.Avatar}.png";

        return new DiscordProfile(discordUser.Id, displayName, avatarUrl);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Checks whether the just-authenticated user belongs to the team's Discord server. Requires the "guilds"
    /// OAuth scope (requested alongside "identify" — see AuthEndpoints).
    /// </summary>
    private async Task<bool> IsMemberOfRequiredGuildAsync(string accessToken, string requiredGuildId, CancellationToken cancellationToken)
    {
        using var guildsRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me/guilds");
        guildsRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

        var guildsResponse = await httpClient.SendAsync(guildsRequest, cancellationToken);

        if (!guildsResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("Pobranie listy serwerów Discord nie powiodło się: {StatusCode}", guildsResponse.StatusCode);
            return false;
        }

        var guilds = await guildsResponse.Content.ReadFromJsonAsync<List<DiscordGuildResponse>>(cancellationToken: cancellationToken);

        return guilds?.Any(g => g.Id == requiredGuildId) ?? false;
    }

    #endregion

    #region Private Types

    private record DiscordTokenResponse([property: JsonPropertyName("access_token")] string AccessToken);

    private record DiscordUserResponse(
        [property: JsonPropertyName("id")] string Id,
        [property: JsonPropertyName("username")] string Username,
        [property: JsonPropertyName("global_name")] string? GlobalName,
        [property: JsonPropertyName("avatar")] string? Avatar);

    private record DiscordGuildResponse([property: JsonPropertyName("id")] string Id);

    #endregion
}
