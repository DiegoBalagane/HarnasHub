#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Common.Jobs;

/// <summary>State transitions of a <see cref="BackgroundJob"/> row. The worker calls each one in its own DI scope (fresh
/// DbContext), so whatever a failed handler left tracked in its context can never leak into the status write.</summary>
public static class JobStateStore
{
	#region Public Methods

	/// <summary>Moves a queued job to Running; null when it doesn't exist or isn't queued any more (nothing to run).</summary>
	public static async Task<BackgroundJob?> TryStartAsync(
		IApplicationDbContext dbContext, Guid jobId, string initialStage, DateTime nowUtc, CancellationToken cancellationToken)
	{
		var job = await dbContext.BackgroundJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
		if (job is null || job.Status != BackgroundJobStatus.Queued)
		{
			return null;
		}

		job.Status = BackgroundJobStatus.Running;
		job.StartedAtUtc = nowUtc;
		job.Progress = 0;
		job.Stage = initialStage;
		await dbContext.SaveChangesAsync(cancellationToken);
		return job;
	}

	/// <summary>Records how the job ended; a succeeded job jumps to 100 %, a failed one keeps the progress it reached.</summary>
	public static async Task<BackgroundJob?> CompleteAsync(
		IApplicationDbContext dbContext, Guid jobId, JobOutcome outcome, DateTime nowUtc, CancellationToken cancellationToken)
	{
		var job = await dbContext.BackgroundJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
		if (job is null)
		{
			return null;
		}

		job.Status = outcome.Succeeded ? BackgroundJobStatus.Succeeded : BackgroundJobStatus.Failed;
		job.ResultJson = outcome.ResultJson;
		job.ErrorMessage = outcome.ErrorMessage;
		job.FinishedAtUtc = nowUtc;
		if (outcome.Succeeded)
		{
			job.Progress = 100;
			job.Stage = null;
		}

		await dbContext.SaveChangesAsync(cancellationToken);
		return job;
	}

	/// <summary>On startup: fails every job a previous process left queued or running (the in-process queue died with it) —
	/// only those created before <paramref name="processStartedAtUtc"/>, so a job queued by this process in the meantime is
	/// never touched — and deletes finished jobs older than <paramref name="retention"/>. Returns how many jobs were failed.</summary>
	public static async Task<int> RecoverInterruptedAsync(
		IApplicationDbContext dbContext, DateTime processStartedAtUtc, DateTime nowUtc, TimeSpan retention, CancellationToken cancellationToken)
	{
		var interrupted = await dbContext.BackgroundJobs
			.Where(j => (j.Status == BackgroundJobStatus.Queued || j.Status == BackgroundJobStatus.Running)
				&& j.CreatedAtUtc < processStartedAtUtc)
			.ToListAsync(cancellationToken);
		foreach (var job in interrupted)
		{
			job.Status = BackgroundJobStatus.Failed;
			job.ErrorMessage = JobOutcome.InterruptedMessage;
			job.FinishedAtUtc = nowUtc;
		}

		var cutoff = nowUtc - retention;
		var expired = await dbContext.BackgroundJobs
			.Where(j => (j.Status == BackgroundJobStatus.Succeeded || j.Status == BackgroundJobStatus.Failed)
				&& j.FinishedAtUtc != null && j.FinishedAtUtc < cutoff)
			.ToListAsync(cancellationToken);
		dbContext.BackgroundJobs.RemoveRange(expired);

		await dbContext.SaveChangesAsync(cancellationToken);
		return interrupted.Count;
	}

	#endregion
}
