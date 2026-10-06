#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Core.Enums;
using HarnasHub.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Infrastructure.Jobs;

/// <summary>Persists and pushes one running job's progress. <see cref="Publish"/> is called from inside stream reads, so it
/// only records the latest value and kicks off a single background flush loop; each write uses its own DI scope (never the
/// handler's DbContext) and only touches a row that is still Running, so a late write can't undo the final status.</summary>
public sealed class JobProgressSink(
	Guid jobId,
	Guid requestedByUserId,
	IServiceScopeFactory scopeFactory,
	IRealtimeNotifier realtimeNotifier,
	ILogger logger) : IJobProgressSink
{
	#region Private Fields

	private const int MaxStageLength = 200;
	private static readonly TimeSpan MinInterval = TimeSpan.FromMilliseconds(750);

	private readonly object _gate = new();
	private readonly ProgressThrottle _throttle = new(MinInterval);
	private (int Percent, string? Stage)? _pending;
	private bool _flushing;
	private Task _flushTask = Task.CompletedTask;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Publish(int percent, string? stage)
	{
		lock (_gate)
		{
			if (!_throttle.ShouldPublish(percent, stage, DateTimeOffset.UtcNow))
			{
				return;
			}

			_pending = (percent, stage);
			if (!_flushing)
			{
				_flushing = true;
				_flushTask = Task.Run(FlushLoopAsync);
			}
		}
	}

	/// <summary>Waits until every published update has been written (called before the final status is stored).</summary>
	public Task DrainAsync()
	{
		lock (_gate)
		{
			return _flushTask;
		}
	}

	#endregion

	#region Private Methods

	private async Task FlushLoopAsync()
	{
		while (true)
		{
			(int Percent, string? Stage) update;
			lock (_gate)
			{
				if (_pending is null)
				{
					_flushing = false;
					return;
				}

				update = _pending.Value;
				_pending = null;
			}

			try
			{
				var stage = update.Stage is { Length: > MaxStageLength } tooLong ? tooLong[..MaxStageLength] : update.Stage;
				await using var scope = scopeFactory.CreateAsyncScope();
				var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
				await dbContext.BackgroundJobs
					.Where(j => j.Id == jobId && j.Status == BackgroundJobStatus.Running)
					.ExecuteUpdateAsync(setters => setters
						.SetProperty(j => j.Progress, update.Percent)
						.SetProperty(j => j.Stage, stage));
				await JobTopics.NotifyAsync(realtimeNotifier, jobId, requestedByUserId);
			}
			catch (Exception ex)
			{
				// Progress is cosmetic — a failed write must never fail the job itself.
				logger.LogWarning(ex, "Nie udało się zapisać postępu zadania {JobId}", jobId);
			}
		}
	}

	#endregion
}
