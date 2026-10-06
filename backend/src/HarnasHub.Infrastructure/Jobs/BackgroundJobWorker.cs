#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Application.Features.Jobs.Shared;
using HarnasHub.Core.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

#endregion

namespace HarnasHub.Infrastructure.Jobs;

/// <summary>Runs queued background jobs: on start it fails jobs a previous process left behind, then keeps
/// <see cref="BackgroundJobOptions.DemoConcurrency"/> / <see cref="BackgroundJobOptions.FaceitConcurrency"/> consumers per lane.
/// Each job uses three DI scopes — mark running, execute (the request goes through MediatR like any other), store outcome —
/// so the handler's DbContext never carries over into the status writes.</summary>
public sealed class BackgroundJobWorker(
	BackgroundJobQueue queue,
	IServiceScopeFactory scopeFactory,
	IRealtimeNotifier realtimeNotifier,
	IOptions<BackgroundJobOptions> options,
	ILogger<BackgroundJobWorker> logger) : BackgroundService
{
	#region Private Fields

	private const string UnknownKindMessage = "Nieznany rodzaj zadania — mogło pochodzić ze starszej wersji aplikacji.";

	// Captured when the singleton is built (before the server accepts requests): recovery only touches older jobs.
	private readonly DateTime _processStartedAtUtc = DateTime.UtcNow;

	#endregion

	#region Protected Methods

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await RecoverAsync(stoppingToken);

		var settings = options.Value;
		var consumers = new List<Task>();
		foreach (var lane in Enum.GetValues<JobLane>())
		{
			var concurrency = Math.Max(1, lane == JobLane.Demo ? settings.DemoConcurrency : settings.FaceitConcurrency);
			for (var i = 0; i < concurrency; i++)
			{
				consumers.Add(ConsumeAsync(lane, stoppingToken));
			}
		}

		await Task.WhenAll(consumers);
	}

	#endregion

	#region Private Methods

	private async Task RecoverAsync(CancellationToken stoppingToken)
	{
		try
		{
			await using var scope = scopeFactory.CreateAsyncScope();
			var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
			var failed = await JobStateStore.RecoverInterruptedAsync(
				dbContext, _processStartedAtUtc, DateTime.UtcNow, TimeSpan.FromDays(Math.Max(1, options.Value.RetentionDays)), stoppingToken);
			if (failed > 0)
			{
				logger.LogWarning("Oznaczono {Count} przerwanych zadań w tle jako nieudane (restart serwera)", failed);
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Nie udało się posprzątać przerwanych zadań w tle");
		}
	}

	private async Task ConsumeAsync(JobLane lane, CancellationToken stoppingToken)
	{
		try
		{
			await foreach (var jobId in queue.Reader(lane).ReadAllAsync(stoppingToken))
			{
				await ProcessAsync(jobId, stoppingToken);
			}
		}
		catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
		{
			// Shutdown — jobs still queued are failed by the next start's recovery.
		}
	}

	private async Task ProcessAsync(Guid jobId, CancellationToken stoppingToken)
	{
		try
		{
			var (job, definition) = await StartAsync(jobId, stoppingToken);
			if (job is null || definition is null)
			{
				return;
			}

			await JobTopics.NotifyAsync(realtimeNotifier, job.Id, job.RequestedByUserId);

			var sink = new JobProgressSink(job.Id, job.RequestedByUserId, scopeFactory, realtimeNotifier, logger);
			JobOutcome outcome;
			await using (var scope = scopeFactory.CreateAsyncScope())
			{
				scope.ServiceProvider.GetRequiredService<JobExecutionContext>().Begin(job.Id, job.RequestedByUserId, sink);
				var sender = scope.ServiceProvider.GetRequiredService<ISender>();
				outcome = await JobRunner.ExecuteAsync(definition, job.PayloadJson, sender, logger, stoppingToken);
			}

			await sink.DrainAsync();
			await CompleteAsync(job.Id, job.RequestedByUserId, outcome);
		}
		catch (Exception ex) when (ex is not OperationCanceledException || !stoppingToken.IsCancellationRequested)
		{
			logger.LogError(ex, "Obsługa zadania w tle {JobId} nie powiodła się", jobId);
		}
	}

	/// <summary>Marks the job running; (null, null) when there's nothing to run. An unknown kind is failed right away.</summary>
	private async Task<(Core.Entities.BackgroundJob? Job, IJobDefinition? Definition)> StartAsync(Guid jobId, CancellationToken stoppingToken)
	{
		await using var scope = scopeFactory.CreateAsyncScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
		var stored = await dbContext.BackgroundJobs.AsNoTracking()
			.Where(j => j.Id == jobId)
			.Select(j => new { j.Kind, j.RequestedByUserId })
			.FirstOrDefaultAsync(stoppingToken);
		if (stored is null)
		{
			return (null, null);
		}

		if (JobRegistry.ByKind(stored.Kind) is not { } definition)
		{
			await CompleteAsync(jobId, stored.RequestedByUserId, JobOutcome.Failure(UnknownKindMessage));
			return (null, null);
		}

		var job = await JobStateStore.TryStartAsync(dbContext, jobId, definition.InitialStage, DateTime.UtcNow, stoppingToken);
		return (job, job is null ? null : definition);
	}

	private async Task CompleteAsync(Guid jobId, Guid requestedByUserId, JobOutcome outcome)
	{
		// Not tied to the stopping token: a job cut short by shutdown must still be recorded as failed.
		await using var scope = scopeFactory.CreateAsyncScope();
		var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
		await JobStateStore.CompleteAsync(dbContext, jobId, outcome, DateTime.UtcNow, CancellationToken.None);
		await JobTopics.NotifyAsync(realtimeNotifier, jobId, requestedByUserId);
	}

	#endregion
}
