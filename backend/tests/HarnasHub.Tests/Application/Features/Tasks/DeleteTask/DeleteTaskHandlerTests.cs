using HarnasHub.Application.Features.Tasks.DeleteTask;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.DeleteTask;

public class DeleteTaskHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_an_existing_task()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var task = new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Przydzielone przez pomyłkę",
			AssignedToUserId = Guid.NewGuid(),
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Todo,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tasks.Add(task);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var realtimeNotifier = new TestRealtimeNotifier();
		var handler = new DeleteTaskHandler(dbContext, realtimeNotifier);

		var result = await handler.Handle(new DeleteTaskCommand(task.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.Tasks);
		Assert.Equal(["tasks", "dashboard"], realtimeNotifier.Topics);
	}

	[Fact]
	public async Task Should_return_an_error_for_an_unknown_task()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteTaskHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteTaskCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.TaskNotFound", result.FirstError.Code);
	}

	#endregion
}
