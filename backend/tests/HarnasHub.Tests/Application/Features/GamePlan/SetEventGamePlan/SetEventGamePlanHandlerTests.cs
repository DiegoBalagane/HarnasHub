using HarnasHub.Application.Features.GamePlan.GetEventGamePlan;
using HarnasHub.Application.Features.GamePlan.SetEventGamePlan;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.GamePlan.SetEventGamePlan;

public class SetEventGamePlanHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_save_notes_and_items_in_the_given_order()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = AddEvent(dbContext);
		var first = AddTactic(dbContext, "Szybkie B", MapName.Mirage);
		var second = AddTactic(dbContext, "Split A", MapName.Nuke);
		var board = AddBoard(dbContext, "Anty-eco CT");
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var userId = Guid.NewGuid();

		var result = await CreateHandler(dbContext, userId).Handle(
			new SetEventGamePlanCommand(eventId, "  Pistol: szybkie B, potem force  ", [second, first], [board]),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Pistol: szybkie B, potem force", result.Value.Notes);
		Assert.Equal(["Split A", "Szybkie B"], result.Value.Tactics.Select(t => t.Name));
		Assert.Equal(("Nuke", "T", "FullBuy"), (result.Value.Tactics[0].MapName, result.Value.Tactics[0].Side, result.Value.Tactics[0].Economy));
		Assert.Equal("Anty-eco CT", Assert.Single(result.Value.Boards).Title);
		Assert.Equal(userId, (await dbContext.EventGamePlans.SingleAsync()).UpdatedByUserId);
	}

	[Fact]
	public async Task Should_replace_the_previous_plan_instead_of_appending()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = AddEvent(dbContext);
		var tactic = AddTactic(dbContext, "Szybkie B", MapName.Mirage);
		var board = AddBoard(dbContext, "Retake B");
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = CreateHandler(dbContext, Guid.NewGuid());
		await handler.Handle(new SetEventGamePlanCommand(eventId, "v1", [tactic], [board]), CancellationToken.None);

		var result = await handler.Handle(new SetEventGamePlanCommand(eventId, " ", [tactic], []), CancellationToken.None);

		Assert.Null(result.Value.Notes);
		Assert.Single(result.Value.Tactics);
		Assert.Empty(result.Value.Boards);
		Assert.Single(await dbContext.EventGamePlans.ToListAsync());
		Assert.Single(await dbContext.EventGamePlanItems.ToListAsync());
	}

	[Fact]
	public async Task Should_reject_unknown_tactics_or_boards()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = AddEvent(dbContext);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await CreateHandler(dbContext, Guid.NewGuid()).Handle(
			new SetEventGamePlanCommand(eventId, null, [Guid.NewGuid()], []),
			CancellationToken.None);

		Assert.Equal("GamePlan.UnknownItems", result.FirstError.Code);
		Assert.Empty(dbContext.EventGamePlans);
	}

	[Fact]
	public async Task Should_skip_items_whose_target_was_deleted_later()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = AddEvent(dbContext);
		var kept = AddTactic(dbContext, "Zostaje", MapName.Mirage);
		var removed = AddTactic(dbContext, "Usunięta", MapName.Mirage);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		await CreateHandler(dbContext, Guid.NewGuid()).Handle(new SetEventGamePlanCommand(eventId, null, [kept, removed], []), CancellationToken.None);
		dbContext.Tactics.Remove(await dbContext.Tactics.SingleAsync(t => t.Id == removed));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetEventGamePlanHandler(dbContext).Handle(new GetEventGamePlanQuery(eventId), CancellationToken.None);

		Assert.Equal("Zostaje", Assert.Single(result.Value.Tactics).Name);
	}

	[Fact]
	public async Task Should_return_an_empty_plan_for_an_event_without_one_and_not_found_for_a_missing_event()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = AddEvent(dbContext);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = new GetEventGamePlanHandler(dbContext);

		var empty = await handler.Handle(new GetEventGamePlanQuery(eventId), CancellationToken.None);
		var missing = await handler.Handle(new GetEventGamePlanQuery(Guid.NewGuid()), CancellationToken.None);
		var missingSet = await CreateHandler(dbContext, Guid.NewGuid()).Handle(
			new SetEventGamePlanCommand(Guid.NewGuid(), null, [], []),
			CancellationToken.None);

		Assert.Null(empty.Value.Notes);
		Assert.Null(empty.Value.UpdatedAtUtc);
		Assert.Empty(empty.Value.Tactics);
		Assert.Equal("GamePlan.EventNotFound", missing.FirstError.Code);
		Assert.Equal("GamePlan.EventNotFound", missingSet.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static SetEventGamePlanHandler CreateHandler(TestApplicationDbContext dbContext, Guid userId) =>
		new(dbContext, new TestCurrentUserService(userId), new TestRealtimeNotifier());

	private static Guid AddEvent(TestApplicationDbContext dbContext)
	{
		var id = Guid.NewGuid();
		dbContext.Events.Add(new Event
		{
			Id = id,
			Title = "Liga",
			Type = EventType.Match,
			StartsAtUtc = DateTime.UtcNow,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		return id;
	}

	private static Guid AddTactic(TestApplicationDbContext dbContext, string name, MapName map)
	{
		var id = Guid.NewGuid();
		dbContext.Tactics.Add(new Tactic
		{
			Id = id,
			Name = name,
			MapName = map,
			Side = MapSide.T,
			Economy = EconomyType.FullBuy,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		return id;
	}

	private static Guid AddBoard(TestApplicationDbContext dbContext, string title)
	{
		var id = Guid.NewGuid();
		dbContext.AnalysisBoards.Add(new AnalysisBoard
		{
			Id = id,
			Title = title,
			MapName = MapName.Mirage,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		});
		return id;
	}

	#endregion
}
