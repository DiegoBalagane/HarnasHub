using HarnasHub.Application.Features.Tasks.UpdateTask;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.UpdateTask;

public class UpdateTaskHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_tasks_content_without_touching_status_or_assignee()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var assigneeId = Guid.NewGuid();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Stary tytuł",
			AssignedToUserId = assigneeId,
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.PendingReview,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateTaskHandler(dbContext, notifier);
		var dueAt = DateTime.UtcNow.AddDays(3);

		var command = new UpdateTaskCommand(task.Id, "Nowy tytuł", "Nowy opis", dueAt, null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Nowy tytuł", result.Value.Title);
		Assert.Equal("Nowy opis", result.Value.Description);
		Assert.Equal(TaskItemStatus.PendingReview.ToString(), result.Value.Status);
		Assert.Equal(assigneeId, task.AssignedToUserId);
		Assert.Equal(["tasks", "dashboard"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_task()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateTaskHandler(dbContext, new TestRealtimeNotifier());

		var command = new UpdateTaskCommand(Guid.NewGuid(), "Tytuł", null, null, null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.TaskNotFound", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_an_error_for_an_unknown_material()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Tytuł",
			AssignedToUserId = Guid.NewGuid(),
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Todo,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateTaskHandler(dbContext, new TestRealtimeNotifier());

		var command = new UpdateTaskCommand(task.Id, "Tytuł", null, null, Guid.NewGuid());
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.MaterialNotFound", result.FirstError.Code);
	}

	#endregion
}
