#region Usings

using System.Text.Json;
using ErrorOr;
using FluentValidation;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Application.Features.Jobs.Shared;
using HarnasHub.Application.Features.Jobs.StartJob;
using HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;
using HarnasHub.Application.Features.Results.GetResults;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Jobs.StartJob;

public class StartJobHandlerTests
{
	#region Private Fields

	private const string DemoKey = "demos/0123456789abcdef0123456789abcdef";

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_store_a_queued_job_of_the_current_user_and_enqueue_it_on_its_lane()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var queue = new RecordingQueue();
		var userId = Guid.NewGuid();
		var matchId = Guid.NewGuid();

		var result = await Handler(dbContext, queue, userId).Handle(
			new StartJobCommand(new AttachDemoToResultCommand(matchId, DemoKey)), CancellationToken.None);

		Assert.False(result.IsError);
		var job = await dbContext.BackgroundJobs.SingleAsync();
		Assert.Equal(result.Value.JobId, job.Id);
		Assert.Equal((JobKinds.AttachDemoToResult, BackgroundJobStatus.Queued, userId), (job.Kind, job.Status, job.RequestedByUserId));
		using var payload = JsonDocument.Parse(job.PayloadJson);
		Assert.Equal(matchId, payload.RootElement.GetProperty("matchResultId").GetGuid());
		Assert.Equal([(job.Id, JobLane.Demo)], queue.Enqueued);
	}

	[Fact]
	public async Task Should_reject_an_invalid_request_without_storing_anything()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var queue = new RecordingQueue();

		var result = await Handler(dbContext, queue, Guid.NewGuid()).Handle(
			new StartJobCommand(new AttachDemoToResultCommand(Guid.NewGuid(), "secrets/other-object")), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal(ErrorType.Validation, result.FirstError.Type);
		Assert.Empty(await dbContext.BackgroundJobs.ToListAsync());
		Assert.Empty(queue.Enqueued);
	}

	[Fact]
	public async Task Should_refuse_a_request_that_is_not_a_registered_job()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new RecordingQueue(), Guid.NewGuid()).Handle(
			new StartJobCommand(new GetResultsQuery()), CancellationToken.None);

		Assert.Equal(JobErrors.UnknownKind, result.FirstError);
	}

	[Fact]
	public void Should_register_every_job_kind_once()
	{
		Assert.Equal(JobRegistry.All.Count, JobRegistry.All.Select(d => d.Kind).Distinct().Count());
		Assert.All(JobRegistry.All, d => Assert.Same(d, JobRegistry.ByKind(d.Kind)));
	}

	#endregion

	#region Private Methods

	private static StartJobHandler Handler(TestApplicationDbContext dbContext, RecordingQueue queue, Guid userId)
	{
		var services = new ServiceCollection()
			.AddSingleton<IValidator<AttachDemoToResultCommand>, AttachDemoToResultCommandValidator>()
			.BuildServiceProvider();
		return new StartJobHandler(dbContext, queue, new TestCurrentUserService(userId, "Manager"), services);
	}

	#endregion

	#region Nested Types

	private sealed class RecordingQueue : IJobQueue
	{
		public List<(Guid JobId, JobLane Lane)> Enqueued { get; } = [];

		public void Enqueue(Guid jobId, JobLane lane) => Enqueued.Add((jobId, lane));
	}

	#endregion
}
