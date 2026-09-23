using HarnasHub.Application.Features.Tasks.SubmitTaskForReview;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.SubmitTaskForReview;

public class SubmitTaskForReviewHandlerTests
{
	#region Public Methods

	[Theory]
	[InlineData(TaskItemStatus.Todo)]
	[InlineData(TaskItemStatus.NeedsRework)]
	public async Task Should_move_a_submittable_task_to_pending_review(TaskItemStatus initialStatus)
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = userId,
			AssignedByUserId = Guid.NewGuid(),
			Status = initialStatus,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var realtimeNotifier = new TestRealtimeNotifier();
		var handler = new SubmitTaskForReviewHandler(dbContext, new TestCurrentUserService(userId), realtimeNotifier);

		var result = await handler.Handle(new SubmitTaskForReviewCommand(task.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(TaskItemStatus.PendingReview, task.Status);
		Assert.Equal(["tasks", "dashboard"], realtimeNotifier.Topics);
	}

	[Fact]
	public async Task Should_reject_submitting_someone_elses_task()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = Guid.NewGuid(),
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Todo,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SubmitTaskForReviewHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());

		var result = await handler.Handle(new SubmitTaskForReviewCommand(task.Id), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.NotYourTask", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_reject_submitting_a_task_that_is_already_pending_review()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = userId,
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.PendingReview,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SubmitTaskForReviewHandler(dbContext, new TestCurrentUserService(userId), new TestRealtimeNotifier());

		var result = await handler.Handle(new SubmitTaskForReviewCommand(task.Id), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.NotSubmittable", result.FirstError.Code);
	}

	#endregion
}
