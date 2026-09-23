using HarnasHub.Application.Features.Tasks.GetAllTasks;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.GetAllTasks;

public class GetAllTasksHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_use_the_custom_nickname_when_the_player_set_one()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var player = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = Guid.NewGuid().ToString(),
			DisplayName = "Discord Nick",
			InGameNickname = "Zawodnik",
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Users.Add(player);
		dbContext.Tasks.Add(new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = player.Id,
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Todo,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetAllTasksHandler(dbContext);

		var result = await handler.Handle(new GetAllTasksQuery(), CancellationToken.None);

		Assert.False(result.IsError);
		var task = Assert.Single(result.Value);
		Assert.Equal(player.Id, task.AssignedToUserId);
		Assert.Equal("Zawodnik", task.AssignedToDisplayName);
	}

	#endregion
}
