using HarnasHub.Application.Features.Tactics.CreateTactic;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.CreateTactic;

public class CreateTacticHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_create_an_empty_tactic_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var currentUser = new TestCurrentUserService(userId, "Manager");
		var notifier = new TestRealtimeNotifier();
		var handler = new CreateTacticHandler(dbContext, currentUser, notifier);

		var command = new CreateTacticCommand(MapName.Mirage, MapSide.T, "Eco rush B", EconomyType.Eco, "Wszyscy na B.");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Eco rush B", result.Value.Name);
		Assert.Equal(userId, result.Value.CreatedByUserId);
		Assert.Empty(result.Value.Points);
		Assert.Equal(["tactics"], notifier.Topics);

		var stored = dbContext.Tactics.Single();
		Assert.Equal(MapName.Mirage, stored.MapName);
		Assert.Equal(MapSide.T, stored.Side);
	}

	#endregion
}
