using HarnasHub.Application.Features.MapStrategy.RemovePlayerPosition;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.RemovePlayerPosition;

public class RemovePlayerPositionHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_remove_the_position_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var positionId = Guid.NewGuid();
		dbContext.MapPositionAssignments.Add(new MapPositionAssignment
		{
			Id = positionId,
			MapName = MapName.Anubis,
			Side = MapSide.CT,
			UserId = Guid.NewGuid(),
			X = 0.5f,
			Y = 0.5f,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new RemovePlayerPositionHandler(dbContext, notifier);

		var result = await handler.Handle(new RemovePlayerPositionCommand(positionId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.MapPositionAssignments.ToListAsync());
		Assert.Equal(["map-strategy"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_position()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new RemovePlayerPositionHandler(dbContext, notifier);

		var result = await handler.Handle(new RemovePlayerPositionCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("MapStrategy.PositionNotFound", result.FirstError.Code);
		Assert.Empty(notifier.Topics);
	}

	#endregion
}
