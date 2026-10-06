using HarnasHub.Application.Features.Veto.GetEventVeto;
using HarnasHub.Application.Features.Veto.SetEventVeto;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.SetEventVeto;

public class SetEventVetoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_replace_the_whole_veto_and_number_steps_in_list_order()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = await AddEventAsync(dbContext);
		var handler = new SetEventVetoHandler(dbContext, new TestRealtimeNotifier());
		await handler.Handle(
			new SetEventVetoCommand(eventId, [new VetoStepInput(VetoActor.Us, VetoAction.Ban, MapName.Cache)]),
			CancellationToken.None);

		var result = await handler.Handle(
			new SetEventVetoCommand(eventId,
			[
				new VetoStepInput(VetoActor.Opponent, VetoAction.Ban, MapName.Nuke),
				new VetoStepInput(VetoActor.Us, VetoAction.Pick, MapName.Mirage),
				new VetoStepInput(VetoActor.Opponent, VetoAction.Decider, MapName.Inferno)
			]),
			CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await new GetEventVetoHandler(dbContext).Handle(new GetEventVetoQuery(eventId), CancellationToken.None);
		Assert.Equal(
			[(1, "Opponent", "Ban", "Nuke"), (2, "Us", "Pick", "Mirage"), (3, "Opponent", "Decider", "Inferno")],
			stored.Value.Select(s => (s.Order, s.Actor, s.Action, s.MapName)));
	}

	[Fact]
	public async Task Should_clear_the_veto_with_an_empty_list()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var eventId = await AddEventAsync(dbContext);
		var handler = new SetEventVetoHandler(dbContext, new TestRealtimeNotifier());
		await handler.Handle(
			new SetEventVetoCommand(eventId, [new VetoStepInput(VetoActor.Us, VetoAction.Ban, MapName.Cache)]),
			CancellationToken.None);

		await handler.Handle(new SetEventVetoCommand(eventId, []), CancellationToken.None);

		Assert.Empty(dbContext.EventVetoSteps);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_event()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var set = await new SetEventVetoHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new SetEventVetoCommand(Guid.NewGuid(), []), CancellationToken.None);
		var get = await new GetEventVetoHandler(dbContext).Handle(new GetEventVetoQuery(Guid.NewGuid()), CancellationToken.None);

		Assert.Equal("Veto.EventNotFound", set.FirstError.Code);
		Assert.Equal("Veto.EventNotFound", get.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static async Task<Guid> AddEventAsync(TestApplicationDbContext dbContext)
	{
		var id = Guid.NewGuid();
		dbContext.Events.Add(new Event
		{
			Id = id,
			Title = "Liga",
			Type = EventType.Match,
			StartsAtUtc = DateTime.UtcNow,
			Opponent = "Team X",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);
		return id;
	}

	#endregion
}
