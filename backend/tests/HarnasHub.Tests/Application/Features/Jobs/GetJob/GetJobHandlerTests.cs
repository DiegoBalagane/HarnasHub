#region Usings

using HarnasHub.Application.Features.Jobs.GetJob;
using HarnasHub.Application.Features.Jobs.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Jobs.GetJob;

public class GetJobHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_status_and_the_raw_result_to_the_requester()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var job = await AddAsync(dbContext, userId, BackgroundJobStatus.Succeeded, "{\"roundsCount\":24}");

		var result = await new GetJobHandler(dbContext, new TestCurrentUserService(userId)).Handle(new GetJobQuery(job.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal((BackgroundJobStatus.Succeeded, 100), (result.Value.Status, result.Value.Progress));
		Assert.Equal(24, result.Value.Result!.Value.GetProperty("roundsCount").GetInt32());
	}

	[Theory]
	[InlineData("Manager", false)]
	[InlineData("Player", true)]
	public async Task Should_let_coach_and_manager_read_any_job(string role, bool isCoach)
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var job = await AddAsync(dbContext, Guid.NewGuid(), BackgroundJobStatus.Running, null);

		var result = await new GetJobHandler(dbContext, new TestCurrentUserService(Guid.NewGuid(), role, isCoach))
			.Handle(new GetJobQuery(job.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.Result);
	}

	[Fact]
	public async Task Should_forbid_another_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var job = await AddAsync(dbContext, Guid.NewGuid(), BackgroundJobStatus.Failed, null);

		var result = await new GetJobHandler(dbContext, new TestCurrentUserService(Guid.NewGuid())).Handle(new GetJobQuery(job.Id), CancellationToken.None);

		Assert.Equal(JobErrors.Forbidden, result.FirstError);
	}

	[Fact]
	public async Task Should_report_a_missing_job()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new GetJobHandler(dbContext, new TestCurrentUserService(Guid.NewGuid())).Handle(new GetJobQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.Equal(JobErrors.NotFound, result.FirstError);
	}

	[Fact]
	public void Validator_should_require_a_job_id()
	{
		Assert.False(new GetJobQueryValidator().Validate(new GetJobQuery(Guid.Empty)).IsValid);
		Assert.True(new GetJobQueryValidator().Validate(new GetJobQuery(Guid.NewGuid())).IsValid);
	}

	#endregion

	#region Private Methods

	private static async Task<BackgroundJob> AddAsync(TestApplicationDbContext dbContext, Guid userId, BackgroundJobStatus status, string? resultJson)
	{
		var job = new BackgroundJob
		{
			Id = Guid.NewGuid(),
			Kind = JobKinds.AttachDemoToResult,
			Status = status,
			Progress = status == BackgroundJobStatus.Succeeded ? 100 : 30,
			RequestedByUserId = userId,
			PayloadJson = "{}",
			ResultJson = resultJson,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.BackgroundJobs.Add(job);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		return job;
	}

	#endregion
}
