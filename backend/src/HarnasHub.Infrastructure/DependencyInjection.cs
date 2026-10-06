using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using HarnasHub.Infrastructure.Auth;
using HarnasHub.Infrastructure.BackgroundServices;
using HarnasHub.Infrastructure.Database;
using HarnasHub.Infrastructure.Demos;
using HarnasHub.Infrastructure.Faceit;
using HarnasHub.Infrastructure.Jobs;
using HarnasHub.Infrastructure.Notifications;
using HarnasHub.Infrastructure.Security;
using HarnasHub.Infrastructure.Storage;
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
		services.Configure<FrontendSettings>(configuration.GetSection(FrontendSettings.SectionName));
		services.Configure<S3Settings>(configuration.GetSection(S3Settings.SectionName));
		services.Configure<FaceitOptions>(configuration.GetSection(FaceitOptions.SectionName));
		services.AddSingleton<IIntegrationSettings, IntegrationSettings>();

		services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
		services.AddHttpClient<IDiscordOAuthClient, DiscordOAuthClient>();

		services.AddHttpClient<IDiscordNotifier, DiscordWebhookNotifier>();
		services.AddHostedService<EventReminderService>();
		services.AddHostedService<PendingTimelineCleanupService>();

		// Without Faceit:ApiKey the client reports IsConfigured = false and the sync service exits at once — the app still starts.
		services.AddHttpClient<IFaceitClient, FaceitClient>(client => client.Timeout = TimeSpan.FromSeconds(30));
		// Without Faceit:DownloadsApiToken it reports IsConfigured = false and the UI offers manual uploads only. Demos are
		// ~100 MB, hence the long timeout.
		services.AddHttpClient<IFaceitDemoDownloader, FaceitDemoDownloader>(client => client.Timeout = TimeSpan.FromMinutes(10));
		services.AddHostedService<FaceitSyncService>();

		// The decorator reports parse progress (bytes read) to the running background job, a no-op outside one.
		services.AddScoped<DemoFileParser>();
		services.AddScoped<IDemoParser, ProgressReportingDemoParser>();

		// Background jobs: in-process channels (lost on restart — the worker fails leftovers on start) + one hosted worker.
		services.Configure<BackgroundJobOptions>(configuration.GetSection(BackgroundJobOptions.SectionName));
		services.AddSingleton<BackgroundJobQueue>();
		services.AddSingleton<IJobQueue>(provider => provider.GetRequiredService<BackgroundJobQueue>());
		services.AddHostedService<BackgroundJobWorker>();

		// Singleton: AmazonS3Client is thread-safe and expensive to construct — one per process, not per request.
		services.AddSingleton<IFileStorage, S3FileStorage>();

		return services;
	}

	#endregion
}
