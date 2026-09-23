using HarnasHub.Application.Features.Tasks.ApproveTask;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.ApproveTask;

public class ApproveTaskHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_mark_a_pending_review_task_as_done()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = Guid.NewGuid(),
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.PendingReview,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var realtimeNotifier = new TestRealtimeNotifier();
		var handler = new ApproveTaskHandler(dbContext, realtimeNotifier);

		var result = await handler.Handle(new ApproveTaskCommand(task.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(TaskItemStatus.Done, task.Status);
		Assert.Equal(["tasks", "dashboard"], realtimeNotifier.Topics);
	}

	[Fact]
	public async Task Should_reject_approving_a_task_that_is_not_pending_review()
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

		var handler = new ApproveTaskHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new ApproveTaskCommand(task.Id), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.NotPendingReview", result.FirstError.Code);
	}

	#endregion
}
