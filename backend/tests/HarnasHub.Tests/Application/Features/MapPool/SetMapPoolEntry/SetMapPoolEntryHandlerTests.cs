using HarnasHub.Application.Features.MapPool.SetMapPoolEntry;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapPool.SetMapPoolEntry;

public class SetMapPoolEntryHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_create_then_update_the_entry_for_a_map()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var handler = new SetMapPoolEntryHandler(dbContext, new TestCurrentUserService(userId), new TestRealtimeNotifier());

		await handler.Handle(new SetMapPoolEntryCommand(MapName.Mirage, MapPoolStatus.Learning, "  Uczymy się CT  "), CancellationToken.None);
		var result = await handler.Handle(new SetMapPoolEntryCommand(MapName.Mirage, MapPoolStatus.Core, " "), CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.MapPoolEntries.SingleAsync();
		Assert.Equal(MapName.Mirage, stored.MapName);
		Assert.Equal(MapPoolStatus.Core, stored.Status);
		Assert.Null(stored.Note);
		Assert.Equal(userId, stored.UpdatedByUserId);
	}

	[Fact]
	public async Task Should_trim_the_note()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetMapPoolEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());

		await handler.Handle(new SetMapPoolEntryCommand(MapName.Nuke, MapPoolStatus.Playable, "  Słaby CT  "), CancellationToken.None);

		Assert.Equal("Słaby CT", (await dbContext.MapPoolEntries.SingleAsync()).Note);
	}

	[Fact]
	public async Task Should_remove_the_entry_when_status_is_cleared()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetMapPoolEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());
		await handler.Handle(new SetMapPoolEntryCommand(MapName.Dust2, MapPoolStatus.Ban, null), CancellationToken.None);

		var result = await handler.Handle(new SetMapPoolEntryCommand(MapName.Dust2, null, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.MapPoolEntries);
	}

	[Fact]
	public async Task Should_succeed_when_clearing_a_map_that_has_no_entry()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetMapPoolEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());

		var result = await handler.Handle(new SetMapPoolEntryCommand(MapName.Cache, null, null), CancellationToken.None);

		Assert.False(result.IsError);
	}

	#endregion
}
