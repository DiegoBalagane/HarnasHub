using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using HarnasHub.Infrastructure.Auth;
using HarnasHub.Infrastructure.BackgroundServices;
using HarnasHub.Infrastructure.Database;
using HarnasHub.Infrastructure.Notifications;
using HarnasHub.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HarnasHub.Infrastructure;

/// <summary>Registers the DbContext, auth, notifications, and options bound to configuration.</summary>
public static class DependencyInjection
{
	#region Public Methods

	public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddDbContext<ApplicationDbContext>(options =>
			options.UseNpgsql(configuration.GetConnectionString("Database")));

		services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

		services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
		services.Configure<DiscordSettings>(configuration.GetSection(DiscordSettings.SectionName));
		services.Configure<DiscordOAuthSettings>(configuration.GetSection(DiscordOAuthSettings.SectionName));
		services.Configure<ReminderSettings>(configuration.GetSection(ReminderSettings.SectionName));

		services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
		services.AddHttpClient<IDiscordOAuthClient, DiscordOAuthClient>();

		services.AddHttpClient<IDiscordNotifier, DiscordWebhookNotifier>();
		services.AddHostedService<EventReminderService>();

		return services;
	}

	#endregion
}
