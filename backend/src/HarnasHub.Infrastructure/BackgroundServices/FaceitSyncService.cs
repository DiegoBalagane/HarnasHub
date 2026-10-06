using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using HarnasHub.Application.Features.OpponentReport.SyncOurFaceit;
using HarnasHub.Core.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure.BackgroundServices;

/// <summary>
/// Every <see cref="FaceitOptions.SyncIntervalHours"/> refreshes the FACEIT cache and report of each linked opponent with an
/// event in the next <see cref="FaceitOptions.UpcomingEventDays"/> days, plus our own team. A no-op while no API key is set.
/// </summary>
public class FaceitSyncService(
	IServiceScopeFactory scopeFactory,
	IOptions<FaceitOptions> options,
	ILogger<FaceitSyncService> logger) : BackgroundService
{
	#region Private Fields

	/// <summary>Grace period after startup so the first sync doesn't compete with migrations and warm-up.</summary>
	private static readonly TimeSpan StartupDelay = TimeSpan.FromMinutes(2);

	#endregion

	#region Protected Methods

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var interval = TimeSpan.FromHours(Math.Max(1, options.Value.SyncIntervalHours));

		if (string.IsNullOrWhiteSpace(options.Value.ApiKey))
		{
			logger.LogInformation("Brak klucza FACEIT API — synchronizacja w tle wyłączona");
			return;
		}

		await Task.Delay(StartupDelay, stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await SyncAsync(stoppingToken);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				logger.LogError(ex, "Błąd podczas synchronizacji danych FACEIT w tle");
			}

			await Task.Delay(interval, stoppingToken);
		}
	}

	#endregion

	#region Private Methods

	/// <summary>One pass: every linked upcoming opponent (which also refreshes us), then our team on its own.</summary>
	private async Task SyncAsync(CancellationToken cancellationToken)
	{
		using var scope = scopeFactory.CreateScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
		var sender = scope.ServiceProvider.GetRequiredService<ISender>();

		var now = DateTime.UtcNow;
		var horizon = now.AddDays(Math.Max(1, options.Value.UpcomingEventDays));
		var upcomingKeys = (await dbContext.Events
				.Where(e => e.Opponent != null && e.StartsAtUtc >= now && e.StartsAtUtc <= horizon)
				.Select(e => e.Opponent!)
				.ToListAsync(cancellationToken))
			.Select(OpponentNames.ToKey)
			.Distinct()
			.ToList();

		var linked = await dbContext.OpponentFaceitLinks
			.Where(l => upcomingKeys.Contains(l.OpponentKey))
			.Select(l => l.DisplayName)
			.ToListAsync(cancellationToken);

		foreach (var opponent in linked)
		{
			var result = await sender.Send(new SyncOpponentFaceitCommand(opponent, IsManual: false), cancellationToken);
			if (result.IsError)
			{
				logger.LogWarning("Synchronizacja FACEIT dla {Opponent} nie powiodła się: {Error}", opponent, result.FirstError.Description);
			}
		}

		// Our players synced moments ago through an opponent are skipped inside, so this costs nothing extra then.
		var ours = await sender.Send(new SyncOurFaceitCommand(), cancellationToken);
		if (ours.IsError)
		{
			logger.LogWarning("Synchronizacja FACEIT naszej drużyny nie powiodła się: {Error}", ours.FirstError.Description);
		}
	}

	#endregion
}
