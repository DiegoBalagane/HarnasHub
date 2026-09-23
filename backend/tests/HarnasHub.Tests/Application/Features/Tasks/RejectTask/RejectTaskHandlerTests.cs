using HarnasHub.Application.Features.Tasks.RejectTask;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.RejectTask;

public class RejectTaskHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_send_a_pending_review_task_back_for_rework()
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
		var handler = new RejectTaskHandler(dbContext, realtimeNotifier);

		var result = await handler.Handle(new RejectTaskCommand(task.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(TaskItemStatus.NeedsRework, task.Status);
		Assert.Equal(["tasks", "dashboard"], realtimeNotifier.Topics);
	}

	[Fact]
	public async Task Should_reject_rejecting_a_task_that_is_not_pending_review()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = Guid.NewGuid(),
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Done,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new RejectTaskHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new RejectTaskCommand(task.Id), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.NotPendingReview", result.FirstError.Code);
	}

	#endregion
}
