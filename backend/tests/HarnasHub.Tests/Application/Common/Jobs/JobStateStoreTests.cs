#region Usings

using HarnasHub.Application.Common.Jobs;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Jobs;

public class JobStateStoreTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_start_only_a_queued_job()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var queued = await AddAsync(dbContext, BackgroundJobStatus.Queued, Now);
		var running = await AddAsync(dbContext, BackgroundJobStatus.Running, Now);

		var started = await JobStateStore.TryStartAsync(dbContext, queued.Id, "Analiza demki", Now, CancellationToken.None);
		var again = await JobStateStore.TryStartAsync(dbContext, running.Id, "Analiza demki", Now, CancellationToken.None);

		Assert.NotNull(started);
		Assert.Equal((BackgroundJobStatus.Running, "Analiza demki", (DateTime?)Now), (started.Status, started.Stage, started.StartedAtUtc));
		Assert.Null(again);
	}

	[Fact]
	public async Task Should_complete_a_success_at_100_percent_and_keep_progress_of_a_failure()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var ok = await AddAsync(dbContext, BackgroundJobStatus.Running, Now, progress: 40);
		var bad = await AddAsync(dbContext, BackgroundJobStatus.Running, Now, progress: 40);

		await JobStateStore.CompleteAsync(dbContext, ok.Id, JobOutcome.Success("{\"a\":1}"), Now, CancellationToken.None);
		await JobStateStore.CompleteAsync(dbContext, bad.Id, JobOutcome.Failure("Błąd"), Now, CancellationToken.None);

		Assert.Equal((BackgroundJobStatus.Succeeded, 100, "{\"a\":1}", (string?)null), (ok.Status, ok.Progress, ok.ResultJson, ok.Stage));
		Assert.Equal((BackgroundJobStatus.Failed, 40, "Błąd"), (bad.Status, bad.Progress, bad.ErrorMessage));
		Assert.Equal(Now, bad.FinishedAtUtc);
	}

	[Fact]
	public async Task Should_fail_leftovers_of_a_previous_process_but_not_jobs_queued_since_start()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var processStart = Now.AddMinutes(-1);
		var leftoverRunning = await AddAsync(dbContext, BackgroundJobStatus.Running, Now.AddHours(-1));
		var leftoverQueued = await AddAsync(dbContext, BackgroundJobStatus.Queued, Now.AddHours(-1));
		var fresh = await AddAsync(dbContext, BackgroundJobStatus.Queued, Now);

		var failed = await JobStateStore.RecoverInterruptedAsync(dbContext, processStart, Now, TimeSpan.FromDays(7), CancellationToken.None);

		Assert.Equal(2, failed);
		Assert.All([leftoverRunning, leftoverQueued], job =>
			Assert.Equal((BackgroundJobStatus.Failed, JobOutcome.InterruptedMessage), (job.Status, job.ErrorMessage)));
		Assert.Equal(BackgroundJobStatus.Queued, fresh.Status);
	}

	[Fact]
	public async Task Should_delete_finished_jobs_older_than_the_retention()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var old = await AddAsync(dbContext, BackgroundJobStatus.Succeeded, Now.AddDays(-10), finishedAtUtc: Now.AddDays(-10));
		var recent = await AddAsync(dbContext, BackgroundJobStatus.Failed, Now.AddDays(-1), finishedAtUtc: Now.AddDays(-1));

		await JobStateStore.RecoverInterruptedAsync(dbContext, Now, Now, TimeSpan.FromDays(7), CancellationToken.None);

		var remaining = await dbContext.BackgroundJobs.Select(j => j.Id).ToListAsync();
		Assert.Equal([recent.Id], remaining);
		Assert.DoesNotContain(old.Id, remaining);
	}

	#endregion

	#region Private Methods

	private static async Task<BackgroundJob> AddAsync(
		TestApplicationDbContext dbContext, BackgroundJobStatus status, DateTime createdAtUtc, int progress = 0, DateTime? finishedAtUtc = null)
	{
		var job = new BackgroundJob
		{
			Id = Guid.NewGuid(),
			Kind = "test",
			Status = status,
			Progress = progress,
			RequestedByUserId = Guid.NewGuid(),
			PayloadJson = "{}",
			CreatedAtUtc = createdAtUtc,
			FinishedAtUtc = finishedAtUtc
		};
		dbContext.BackgroundJobs.Add(job);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		return job;
	}

	#endregion
}
