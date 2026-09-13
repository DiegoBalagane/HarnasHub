namespace HarnasHub.Core.Options;

/// <summary>Configuration for "Sign in with Discord" (OAuth2 authorization code flow).</summary>
public class DiscordOAuthSettings
{
    public const string SectionName = "DiscordOAuth";

    /// <summary>Application Client ID from the Discord Developer Portal. Public — safe to expose.</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>Application Client Secret. Never commit this — use user-secrets locally, an env var in production.</summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>This backend's own callback URL, must exactly match a Redirect URI registered in the Discord app.</summary>
    public string RedirectUri { get; set; } = string.Empty;

    /// <summary>Where to send the browser after login. Empty = same origin (production single-deploy model).</summary>
    public string FrontendCallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// The team's Discord server (guild) ID. When set, only members of this server can sign in — anyone else's
    /// OAuth exchange is rejected even though Discord itself authenticated them. Empty = no restriction (any
    /// Discord account can sign in), which is only reasonable for local dev/testing.
    /// </summary>
    public string? RequiredGuildId { get; set; }
}
