using HarnasHub.Application.Features.Tasks.AssignTask;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tasks.AssignTask;

public class AssignTaskHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_attach_the_material_when_one_is_given()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var assignee = await AddUserAsync(dbContext, "Zenon");
		var material = new TrainingMaterial
		{
			Id = Guid.NewGuid(),
			Title = "VOD review — mirage",
			Url = "https://example.com/vod",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.TrainingMaterials.Add(material);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var currentUser = new TestCurrentUserService(Guid.NewGuid(), "Manager");
		var discordNotifier = new TestDiscordNotifier();
		var realtimeNotifier = new TestRealtimeNotifier();
		var handler = new AssignTaskHandler(dbContext, currentUser, discordNotifier, realtimeNotifier);

		var command = new AssignTaskCommand("Obejrzyj demo", null, assignee, null, material.Id);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(material.Id, result.Value.TrainingMaterialId);
		Assert.Equal(material.Title, result.Value.TrainingMaterialTitle);
		Assert.Equal(material.Url, result.Value.TrainingMaterialUrl);
		Assert.Equal(["tasks", "dashboard"], realtimeNotifier.Topics);
		Assert.Single(discordNotifier.Messages);
	}

	[Fact]
	public async Task Should_create_a_task_without_a_material_when_none_is_given()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var assignee = await AddUserAsync(dbContext, "Zenon");
		var currentUser = new TestCurrentUserService(Guid.NewGuid(), "Manager");
		var handler = new AssignTaskHandler(dbContext, currentUser, new TestDiscordNotifier(), new TestRealtimeNotifier());

		var command = new AssignTaskCommand("Rozgrzewka", null, assignee, null, null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.TrainingMaterialId);
		Assert.Null(result.Value.TrainingMaterialTitle);
	}

	[Fact]
	public async Task Should_return_an_error_for_an_unknown_material()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var assignee = await AddUserAsync(dbContext, "Zenon");
		var currentUser = new TestCurrentUserService(Guid.NewGuid(), "Manager");
		var handler = new AssignTaskHandler(dbContext, currentUser, new TestDiscordNotifier(), new TestRealtimeNotifier());

		var command = new AssignTaskCommand("Obejrzyj demo", null, assignee, null, Guid.NewGuid());
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tasks.MaterialNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static async Task<Guid> AddUserAsync(TestApplicationDbContext dbContext, string displayName)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = Guid.NewGuid().ToString(),
			DisplayName = displayName,
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		return user.Id;
	}

	#endregion
}
