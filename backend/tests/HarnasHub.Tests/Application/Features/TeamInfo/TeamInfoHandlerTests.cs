using HarnasHub.Application.Features.TeamInfo.CreateTeamInfoEntry;
using HarnasHub.Application.Features.TeamInfo.DeleteTeamInfoEntry;
using HarnasHub.Application.Features.TeamInfo.GetTeamInfo;
using HarnasHub.Application.Features.TeamInfo.ReorderTeamInfoEntries;
using HarnasHub.Application.Features.TeamInfo.UpdateTeamInfoEntry;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.TeamInfo;

public class TeamInfoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Create_should_trim_and_append_to_the_end_of_the_category()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var handler = new CreateTeamInfoEntryHandler(dbContext, new TestCurrentUserService(userId), new TestRealtimeNotifier());

		await handler.Handle(new CreateTeamInfoEntryCommand("Serwery", "Trening", "  connect 1.2.3.4:27015 ", false), CancellationToken.None);
		var result = await handler.Handle(new CreateTeamInfoEntryCommand(" Serwery ", "Test", "5.6.7.8:27015", true), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(1, result.Value.SortOrder);
		Assert.True(result.Value.IsSecret);
		var first = await dbContext.TeamInfoEntries.SingleAsync(e => e.Title == "Trening");
		Assert.Equal("connect 1.2.3.4:27015", first.Value);
		Assert.Equal(0, first.SortOrder);
		Assert.Equal(userId, first.UpdatedByUserId);
	}

	[Fact]
	public async Task Get_should_order_by_category_then_sort_order()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var create = new CreateTeamInfoEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());
		await create.Handle(new CreateTeamInfoEntryCommand("Serwery", "B", "b", false), CancellationToken.None);
		await create.Handle(new CreateTeamInfoEntryCommand("Discord", "D", "d", false), CancellationToken.None);
		await create.Handle(new CreateTeamInfoEntryCommand("Serwery", "A", "a", false), CancellationToken.None);

		var result = await new GetTeamInfoHandler(dbContext).Handle(new GetTeamInfoQuery(), CancellationToken.None);

		Assert.Equal(["D", "B", "A"], result.Value.Select(e => e.Title));
	}

	[Fact]
	public async Task Update_should_change_fields_and_move_to_end_of_new_category()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = new TestCurrentUserService(Guid.NewGuid());
		var create = new CreateTeamInfoEntryHandler(dbContext, user, new TestRealtimeNotifier());
		var created = (await create.Handle(new CreateTeamInfoEntryCommand("Serwery", "Trening", "x", false), CancellationToken.None)).Value;
		await create.Handle(new CreateTeamInfoEntryCommand("Inne", "Coś", "y", false), CancellationToken.None);

		var result = await new UpdateTeamInfoEntryHandler(dbContext, user, new TestRealtimeNotifier())
			.Handle(new UpdateTeamInfoEntryCommand(created.Id, "Inne", " Nowy ", "z", true), CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.TeamInfoEntries.SingleAsync(e => e.Id == created.Id);
		Assert.Equal("Inne", stored.Category);
		Assert.Equal("Nowy", stored.Title);
		Assert.True(stored.IsSecret);
		Assert.Equal(1, stored.SortOrder);
	}

	[Fact]
	public async Task Update_should_return_not_found_for_unknown_entry()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateTeamInfoEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateTeamInfoEntryCommand(Guid.NewGuid(), "A", "B", "C", false), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("TeamInfo.EntryNotFound", result.FirstError.Code);
	}

	[Fact]
	public async Task Delete_should_remove_the_entry_or_report_not_found()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var created = (await new CreateTeamInfoEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier())
			.Handle(new CreateTeamInfoEntryCommand("Inne", "T", "V", false), CancellationToken.None)).Value;
		var handler = new DeleteTeamInfoEntryHandler(dbContext, new TestRealtimeNotifier());

		var deleted = await handler.Handle(new DeleteTeamInfoEntryCommand(created.Id), CancellationToken.None);
		var again = await handler.Handle(new DeleteTeamInfoEntryCommand(created.Id), CancellationToken.None);

		Assert.False(deleted.IsError);
		Assert.Empty(dbContext.TeamInfoEntries);
		Assert.True(again.IsError);
	}

	[Fact]
	public async Task Reorder_should_rewrite_sort_order_to_match_the_given_ids()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var create = new CreateTeamInfoEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier());
		var a = (await create.Handle(new CreateTeamInfoEntryCommand("S", "A", "a", false), CancellationToken.None)).Value;
		var b = (await create.Handle(new CreateTeamInfoEntryCommand("S", "B", "b", false), CancellationToken.None)).Value;
		var c = (await create.Handle(new CreateTeamInfoEntryCommand("S", "C", "c", false), CancellationToken.None)).Value;

		var result = await new ReorderTeamInfoEntriesHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new ReorderTeamInfoEntriesCommand([c.Id, a.Id, b.Id]), CancellationToken.None);

		Assert.False(result.IsError);
		var ordered = await dbContext.TeamInfoEntries.OrderBy(e => e.SortOrder).Select(e => e.Title).ToListAsync();
		Assert.Equal(["C", "A", "B"], ordered);
	}

	[Fact]
	public async Task Reorder_should_fail_and_change_nothing_when_an_id_is_unknown()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var a = (await new CreateTeamInfoEntryHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestRealtimeNotifier())
			.Handle(new CreateTeamInfoEntryCommand("S", "A", "a", false), CancellationToken.None)).Value;

		var result = await new ReorderTeamInfoEntriesHandler(dbContext, new TestRealtimeNotifier())
			.Handle(new ReorderTeamInfoEntriesCommand([Guid.NewGuid(), a.Id]), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal(0, (await dbContext.TeamInfoEntries.SingleAsync()).SortOrder);
	}

	#endregion
}
