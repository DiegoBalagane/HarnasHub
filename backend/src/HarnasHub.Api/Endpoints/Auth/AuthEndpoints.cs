using HarnasHub.Api.Common;
using HarnasHub.Application.Features.Auth.DiscordLogin;
using HarnasHub.Application.Features.Auth.RefreshSession;
using HarnasHub.Core.Options;
using MediatR;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace HarnasHub.Api.Endpoints.Auth;

/// <summary>"Sign in with Discord" and session endpoints under /api/auth.</summary>
public class AuthEndpoints : IEndpoint
{
	#region Public Methods

	public static void MapEndpoints(IEndpointRouteBuilder app)
	{
		// No role requirement — deliberately open to every authenticated user, Guest included, so a
		// freshly-promoted account can pick up its new role without a full Discord relogin (see RefreshSessionHandler).
		app.MapPost("/api/auth/refresh", async (ISender sender, CancellationToken cancellationToken) =>
		{
			var result = await sender.Send(new RefreshSessionCommand(), cancellationToken);

			return result.Match(
				success => Results.Ok(success),
				errors => errors.ToProblemResult());
		}).WithTags("Auth").RequireAuthorization().RequireRateLimiting(RateLimitPolicies.Auth);

		var group = app.MapGroup("/api/auth/discord").WithTags("Auth").RequireRateLimiting(RateLimitPolicies.Auth);

		group.MapGet("/login", (HttpContext http, IOptions<DiscordOAuthSettings> settings) =>
		{
			var options = settings.Value;

			// Anti-CSRF "state": random value bound to this browser via a short-lived cookie and echoed back by Discord.
			var state = Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
			http.Response.Cookies.Append(OAuthStateCookie, state, new CookieOptions
			{
				HttpOnly = true,
				Secure = http.Request.IsHttps,
				SameSite = SameSiteMode.Lax,
				MaxAge = TimeSpan.FromMinutes(10),
				Path = "/api/auth/discord"
			});

			var authorizeUrl = QueryHelpers.AddQueryString("https://discord.com/oauth2/authorize", new Dictionary<string, string?>
			{
				["client_id"] = options.ClientId,
				["redirect_uri"] = options.RedirectUri,
				["response_type"] = "code",
				["scope"] = "identify guilds",
				["state"] = state
			});

			return Results.Redirect(authorizeUrl);
		});

		group.MapGet("/callback", async (HttpContext http, string? code, string? error, string? state, ISender sender, IOptions<DiscordOAuthSettings> settings, CancellationToken cancellationToken) =>
		{
			var frontendUrl = settings.Value.FrontendCallbackUrl;

			var expectedState = http.Request.Cookies[OAuthStateCookie];
			http.Response.Cookies.Delete(OAuthStateCookie, new CookieOptions { Path = "/api/auth/discord" });

			if (error is not null || code is null)
			{
				return Results.Redirect($"{frontendUrl}/login?error=discord_denied");
			}

			if (!IsStateValid(expectedState, state))
			{
				return Results.Redirect($"{frontendUrl}/login?error=discord_failed");
			}

			var result = await sender.Send(new DiscordLoginCommand(code), cancellationToken);

			return result.Match(
				success => Results.Redirect($"{frontendUrl}/auth/callback#token={Uri.EscapeDataString(success.AccessToken)}"),
				errors => Results.Redirect($"{frontendUrl}/login?error=discord_failed"));
		});
	}

	/// <summary>Compares the OAuth state cookie with the state echoed by Discord in constant time; both must be present.</summary>
	public static bool IsStateValid(string? expected, string? actual)
	{
		if (string.IsNullOrEmpty(expected) || string.IsNullOrEmpty(actual))
		{
			return false;
		}

		return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
			System.Text.Encoding.UTF8.GetBytes(expected),
			System.Text.Encoding.UTF8.GetBytes(actual));
	}

	#endregion

	#region Private Fields

	private const string OAuthStateCookie = "hh_oauth_state";

	#endregion
}
