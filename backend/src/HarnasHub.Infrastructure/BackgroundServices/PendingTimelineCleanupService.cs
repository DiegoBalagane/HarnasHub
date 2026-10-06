#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Infrastructure.BackgroundServices;

/// <summary>Hourly sweep of timelines parked by the analyse step that no result ever claimed (the coach analysed a demo
/// and then abandoned the form) — anything older than <see cref="MatchTimelineStorage.PendingRetention"/> is deleted.</summary>
public class PendingTimelineCleanupService(IFileStorage fileStorage, ILogger<PendingTimelineCleanupService> logger) : BackgroundService
{
	#region Private Fields

	private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

	#endregion

	#region Protected Methods

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return;
		}

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var cutoff = DateTime.UtcNow - MatchTimelineStorage.PendingRetention;
				var deleted = await fileStorage.DeleteOlderThanAsync(MatchTimelineStorage.PendingPrefix, cutoff, stoppingToken);
				if (deleted > 0)
				{
					logger.LogInformation("Usunięto {Count} nieodebranych tymczasowych osi czasu meczów", deleted);
				}
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				logger.LogWarning(ex, "Błąd podczas czyszczenia tymczasowych osi czasu meczów");
			}

			await Task.Delay(Interval, stoppingToken);
		}
	}

	#endregion
}
