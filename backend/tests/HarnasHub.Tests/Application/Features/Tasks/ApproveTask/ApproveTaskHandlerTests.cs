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
	public async Task Should_mark_a_todo_task_as_done_directly_without_review()
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

		Assert.False(result.IsError);
		Assert.Equal(TaskItemStatus.Done, task.Status);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_task()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new ApproveTaskHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new ApproveTaskCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.TaskNotFound", result.FirstError.Code);
	}

	#endregion
}
