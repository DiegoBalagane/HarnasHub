using HarnasHub.Application.Features.Tasks.GetMyTasks;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Tasks.GetMyTasks;

public class GetMyTasksHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_join_in_the_attached_materials_title_and_url()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var material = new TrainingMaterial
		{
			Id = Guid.NewGuid(),
			Title = "VOD review — mirage",
			Url = "https://example.com/vod",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.TrainingMaterials.Add(material);
		dbContext.Tasks.Add(new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Obejrzyj demo",
			AssignedToUserId = userId,
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Todo,
			CreatedAtUtc = DateTime.UtcNow,
			TrainingMaterialId = material.Id
		});
		dbContext.Tasks.Add(new TaskItem
		{
			Id = Guid.NewGuid(),
			Title = "Rozgrzewka",
			AssignedToUserId = userId,
			AssignedByUserId = Guid.NewGuid(),
			Status = TaskItemStatus.Todo,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetMyTasksHandler(dbContext, new TestCurrentUserService(userId));

		var result = await handler.Handle(new GetMyTasksQuery(), CancellationToken.None);

		Assert.False(result.IsError);
		var withMaterial = result.Value.Single(t => t.Title == "Obejrzyj demo");
		Assert.Equal(material.Title, withMaterial.TrainingMaterialTitle);
		Assert.Equal(material.Url, withMaterial.TrainingMaterialUrl);

		var withoutMaterial = result.Value.Single(t => t.Title == "Rozgrzewka");
		Assert.Null(withoutMaterial.TrainingMaterialId);
		Assert.Null(withoutMaterial.TrainingMaterialTitle);
	}

	#endregion
}
