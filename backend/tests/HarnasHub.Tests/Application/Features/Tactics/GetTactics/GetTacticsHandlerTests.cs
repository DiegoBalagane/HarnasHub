using HarnasHub.Application.Features.Tactics.GetTactics;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.GetTactics;

public class GetTacticsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_filter_by_map_side_and_economy()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddTactic(dbContext, MapName.Mirage, MapSide.T, EconomyType.Eco, "Eco rush B");
		AddTactic(dbContext, MapName.Mirage, MapSide.CT, EconomyType.Eco, "Retake A");
		AddTactic(dbContext, MapName.Inferno, MapSide.T, EconomyType.Eco, "Inferno default");
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetTacticsHandler(dbContext);

		var result = await handler.Handle(new GetTacticsQuery(MapName.Mirage, MapSide.T, EconomyType.Eco), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["Eco rush B"], result.Value.Select(t => t.Name));
	}

	[Fact]
	public async Task Should_return_the_point_count_without_loading_full_points()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var tactic = AddTactic(dbContext, MapName.Nuke, MapSide.CT, EconomyType.FullBuy, "Default CT");
		tactic.Points.Add(new TacticPoint { Id = Guid.NewGuid(), TacticId = tactic.Id, Order = 0, X = 0.1f, Y = 0.1f });
		tactic.Points.Add(new TacticPoint { Id = Guid.NewGuid(), TacticId = tactic.Id, Order = 1, X = 0.2f, Y = 0.2f });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetTacticsHandler(dbContext);

		var result = await handler.Handle(new GetTacticsQuery(null, null, null), CancellationToken.None);

		Assert.Equal(2, result.Value.Single().PointCount);
	}

	#endregion

	#region Private Methods

	private static Tactic AddTactic(TestApplicationDbContext dbContext, MapName mapName, MapSide side, EconomyType economy, string name)
	{
		var tactic = new Tactic
		{
			Id = Guid.NewGuid(),
			MapName = mapName,
			Side = side,
			Economy = economy,
			Name = name,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};

		dbContext.Tactics.Add(tactic);
		return tactic;
	}

	#endregion
}
