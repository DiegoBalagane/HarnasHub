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

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
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
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();

/// <summary>Entry point class, exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program;
