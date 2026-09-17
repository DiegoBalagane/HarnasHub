using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Application.Features.Tactics.UpdateTactic;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.UpdateTactic;

public class UpdateTacticHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_replace_metadata_and_the_whole_point_layout()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var tacticId = Guid.NewGuid();
		var tactic = new Tactic
		{
			Id = tacticId,
			MapName = MapName.Mirage,
			Side = MapSide.T,
			Name = "Old name",
			Economy = EconomyType.FullBuy,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		tactic.Points.Add(new TacticPoint { Id = Guid.NewGuid(), TacticId = tacticId, Order = 0, X = 0.1f, Y = 0.1f });
		dbContext.Tactics.Add(tactic);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateTacticHandler(dbContext, notifier);

		var newPoints = new List<TacticPointInput>
		{
			new(0.2f, 0.3f, "Smoke z Palace", null),
			new(0.4f, 0.5f, "Flasha na window", null)
		};
		var command = new UpdateTacticCommand(tacticId, "Eco rush B", EconomyType.Eco, "Nowa notatka", newPoints);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Eco rush B", result.Value.Name);
		Assert.Equal("Eco", result.Value.Economy);
		Assert.Equal(2, result.Value.Points.Count);
		Assert.Equal([0, 1], result.Value.Points.Select(p => p.Order));
		Assert.Equal(["tactics"], notifier.Topics);
		Assert.Equal(2, dbContext.TacticPoints.Count());
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_tactic()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateTacticHandler(dbContext, notifier);

		var command = new UpdateTacticCommand(Guid.NewGuid(), "Name", EconomyType.Eco, null, []);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tactics.NotFound", result.FirstError.Code);
		Assert.Empty(notifier.Topics);
	}

	#endregion
}
