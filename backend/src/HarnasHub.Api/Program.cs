using System.Text;
using System.Text.Json.Serialization;
using HarnasHub.Api.Common;
using HarnasHub.Api.Endpoints;
using HarnasHub.Api.Hubs;
using HarnasHub.Application;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using HarnasHub.Infrastructure;
using HarnasHub.Infrastructure.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
	?? throw new InvalidOperationException("Sekcja konfiguracji 'Jwt' jest wymagana.");

// A missing/empty secret would silently sign every token with an empty key — every restart
// would then invalidate all sessions, and anyone could forge a token. Fail loudly instead.
if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
	throw new InvalidOperationException(
		"Konfiguracja 'Jwt:Secret' jest pusta. Ustaw stałą wartość zmiennej środowiskowej Jwt__Secret " +
		"(ta sama wartość na każdym środowisku/restarcie, inaczej wszystkie sesje wygasają przy każdym deployu).");
}

builder.Services
	.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidateAudience = true,
			ValidateLifetime = true,
			ValidateIssuerSigningKey = true,
			ValidIssuer = jwtSettings.Issuer,
			ValidAudience = jwtSettings.Audience,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
		};

		// SignalR's browser transports can't set an Authorization header, so accept the JWT via query string for hub requests.
		options.Events = new JwtBearerEvents
		{
			OnMessageReceived = context =>
			{
				var accessToken = context.Request.Query["access_token"];

				if (!string.IsNullOrEmpty(accessToken) && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
				{
					context.Token = accessToken;
				}

				return Task.CompletedTask;
			}
		};
	});

// Guests (freshly signed-in accounts awaiting a Manager's decision) are authenticated but must not reach any team data.
builder.Services.AddAuthorization(options =>
	options.AddPolicy(AuthorizationPolicies.TeamMember, policy => policy.RequireRole("Player", "Coach", "Manager")));

builder.Services.AddCors(options =>
{
	options.AddPolicy("Frontend", policy =>
	{
		var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
		policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
	});
});

// The app sits behind a platform proxy (Railway): trust its X-Forwarded-* headers so rate limiting
// partitions by the real client IP and HTTPS redirection sees the original scheme.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
	options.KnownIPNetworks.Clear();
	options.KnownProxies.Clear();
});

// Auth endpoints are a brute-force/abuse target; a generous global per-client cap protects the costly ones (demo parsing, FACEIT).
builder.Services.AddRateLimiter(options =>
{
	options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

	options.AddPolicy(RateLimitPolicies.Auth, context => RateLimitPartition.GetFixedWindowLimiter(
		context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
		_ => new FixedWindowRateLimiterOptions { PermitLimit = 30, Window = TimeSpan.FromMinutes(1) }));

	options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
		RateLimitPartition.GetFixedWindowLimiter(
			context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value is { Length: > 0 } userId
				? $"user:{userId}"
				: $"ip:{context.Connection.RemoteIpAddress}",
			_ => new FixedWindowRateLimiterOptions { PermitLimit = 600, Window = TimeSpan.FromMinutes(1) }));
});

builder.Services.ConfigureHttpJsonOptions(options =>
	options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Applies pending migrations on boot so a fresh deploy never needs a manual "dotnet ef database update" step.
using (var scope = app.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
	await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}

app.UseHttpsRedirection();

// CSP ships as Report-Only first: it lists the origins the SPA really uses (YouTube embeds, Discord avatars,
// presigned S3 uploads) and is switched to enforcing once the browser console shows no violations.
var s3Endpoint = builder.Configuration["S3:Endpoint"];
var s3Origin = Uri.TryCreate(s3Endpoint, UriKind.Absolute, out var s3Uri) ? s3Uri.GetLeftPart(UriPartial.Authority) : null;
var contentSecurityPolicy = string.Join("; ",
	"default-src 'self'",
	"script-src 'self'",
	"style-src 'self' 'unsafe-inline'",
	"img-src 'self' data: blob: https://cdn.discordapp.com https://img.youtube.com",
	"font-src 'self' data:",
	$"connect-src 'self' {s3Origin}".TrimEnd(),
	"frame-src https://www.youtube-nocookie.com https://www.youtube.com",
	"worker-src 'self'",
	"manifest-src 'self'",
	"object-src 'none'",
	"base-uri 'self'",
	"form-action 'self'",
	"frame-ancestors 'none'");

// Baseline hardening headers.
app.Use(async (context, next) =>
{
	var headers = context.Response.Headers;
	headers["Content-Security-Policy-Report-Only"] = contentSecurityPolicy;
	headers["X-Content-Type-Options"] = "nosniff";
	headers["X-Frame-Options"] = "DENY";
	headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
	headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=()";

	await next();
});

app.UseCors("Frontend");
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

// Some client-side proxies (VPN extensions, corporate caches) will otherwise cache API
// GET responses (e.g. the Discord OAuth redirect) and keep serving a stale one forever.
app.Use(async (context, next) =>
{
	if (context.Request.Path.StartsWithSegments("/api"))
	{
		context.Response.Headers.CacheControl = "no-store";
	}

	await next();
});

app.MapAllEndpoints();
app.MapHub<TeamHub>("/hubs/team");

// Serves the built React app (see docs/DEPLOYMENT.md) so the whole product is a single deployable unit.
// A local `dotnet run` with no wwwroot/ built yet just serves nothing here — API/hub routes are unaffected.
//
// The shell files (index.html, the service worker, its manifest) must never sit in a browser/CDN cache:
// the service worker only discovers a new deploy by re-fetching sw.js and byte-comparing it, and a plain
// F5 only picks up a new build if index.html itself is revalidated — a cached copy of either one is
// exactly how someone can sit on a stale build for hours and see it "fixed" only by Ctrl+Shift+R. Content-
// hashed files under /assets/ are the opposite case: their filename changes whenever their content does,
// so they're safe to cache forever. Everything else (icons, map radars) isn't hashed but rarely changes,
// so a short cache still avoids most repeat requests without risking a long-lived stale copy.
var staticFileOptions = new StaticFileOptions
{
	OnPrepareResponse = context =>
	{
		var path = context.File.Name;
		var isShellFile = path is "index.html" or "sw.js" or "registerSW.js" or "manifest.webmanifest";
		var isHashedAsset = context.Context.Request.Path.StartsWithSegments("/assets");

		context.Context.Response.Headers.CacheControl = isShellFile
			? "no-cache"
			: isHashedAsset
				? "public,max-age=31536000,immutable"
				: "public,max-age=3600";
	},
};

app.UseDefaultFiles();
app.UseStaticFiles(staticFileOptions);
app.MapFallbackToFile("index.html", staticFileOptions);

app.Run();

/// <summary>Entry point class, exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
