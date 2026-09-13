using HarnasHub.Application.Features.Auth.DiscordLogin;
using HarnasHub.Core.Options;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HarnasHub.Api.Endpoints.Auth;

/// <summary>"Sign in with Discord" endpoints under /api/auth/discord.</summary>
public class AuthEndpoints : IEndpoint
{
    #region Public Methods

    public static void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth/discord").WithTags("Auth");

        group.MapGet("/login", (IOptions<DiscordOAuthSettings> settings) =>
        {
            var options = settings.Value;
            var authorizeUrl = QueryHelpers.AddQueryString("https://discord.com/oauth2/authorize", new Dictionary<string, string?>
            {
                ["client_id"] = options.ClientId,
                ["redirect_uri"] = options.RedirectUri,
                ["response_type"] = "code",
                ["scope"] = "identify guilds"
            });

            return Results.Redirect(authorizeUrl);
        });

        group.MapGet("/callback", async (string? code, string? error, ISender sender, IOptions<DiscordOAuthSettings> settings, CancellationToken cancellationToken) =>
        {
            var frontendUrl = settings.Value.FrontendCallbackUrl;

            if (error is not null || code is null)
            {
                return Results.Redirect($"{frontendUrl}/login?error=discord_denied");
            }

            var result = await sender.Send(new DiscordLoginCommand(code), cancellationToken);

            return result.Match(
                success => Results.Redirect($"{frontendUrl}/auth/callback#token={Uri.EscapeDataString(success.AccessToken)}"),
                errors => Results.Redirect($"{frontendUrl}/login?error=discord_failed"));
        });
    }

    #endregion
}
