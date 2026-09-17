using HarnasHub.Application.Features.Tactics.DeleteTactic;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.DeleteTactic;

public class DeleteTacticHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_remove_the_tactic_and_its_points_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var tacticId = Guid.NewGuid();
		var tactic = new Tactic
		{
			Id = tacticId,
			MapName = MapName.Cache,
			Side = MapSide.T,
			Name = "Rush A",
			Economy = EconomyType.Eco,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		tactic.Points.Add(new TacticPoint { Id = Guid.NewGuid(), TacticId = tacticId, Order = 0, X = 0.1f, Y = 0.1f });
		dbContext.Tactics.Add(tactic);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new DeleteTacticHandler(dbContext, notifier);

		var result = await handler.Handle(new DeleteTacticCommand(tacticId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.Tactics.ToListAsync());
		Assert.Empty(await dbContext.TacticPoints.ToListAsync());
		Assert.Equal(["tactics"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_tactic()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new DeleteTacticHandler(dbContext, notifier);

		var result = await handler.Handle(new DeleteTacticCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tactics.NotFound", result.FirstError.Code);
		Assert.Empty(notifier.Topics);
	}

	#endregion
}
