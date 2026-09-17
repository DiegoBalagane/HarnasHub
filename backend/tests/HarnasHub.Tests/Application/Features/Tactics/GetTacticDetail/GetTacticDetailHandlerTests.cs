using HarnasHub.Application.Features.Tactics.GetTacticDetail;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tactics.GetTacticDetail;

public class GetTacticDetailHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_the_tactic_with_points_in_order()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var tacticId = Guid.NewGuid();
		var tactic = new Tactic
		{
			Id = tacticId,
			MapName = MapName.Ancient,
			Side = MapSide.CT,
			Name = "Retake A",
			Economy = EconomyType.FullBuy,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		tactic.Points.Add(new TacticPoint { Id = Guid.NewGuid(), TacticId = tacticId, Order = 1, X = 0.4f, Y = 0.4f, Description = "Drugi" });
		tactic.Points.Add(new TacticPoint { Id = Guid.NewGuid(), TacticId = tacticId, Order = 0, X = 0.2f, Y = 0.2f, Description = "Pierwszy" });
		dbContext.Tactics.Add(tactic);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetTacticDetailHandler(dbContext);

		var result = await handler.Handle(new GetTacticDetailQuery(tacticId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["Pierwszy", "Drugi"], result.Value.Points.Select(p => p.Description));
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_tactic()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new GetTacticDetailHandler(dbContext);

		var result = await handler.Handle(new GetTacticDetailQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tactics.NotFound", result.FirstError.Code);
	}

	#endregion
}
