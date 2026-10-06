#region Usings

using HarnasHub.Application.Features.Tactics.ImportTacticFromDemo;
using HarnasHub.Application.Features.Tactics.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.ImportTacticFromDemo;

/// <summary>Covers saving demo grenades as a tactic, with and without adding them to the nade library.</summary>
public class ImportTacticFromDemoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_create_points_at_landings_ordered_by_time_without_touching_the_library()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = CreateHandler(dbContext, notifier);

		var command = Command(addToLibrary: false,
			Nade(GrenadeType.Flash, 30f, landX: 0.7f),
			Nade(GrenadeType.Smoke, 14.2f, landX: 0.3f));
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["Smoke · kacper · 0:14", "Flash · kacper · 0:30"], result.Value.Points.Select(p => p.Description));
		Assert.Equal([0, 1], result.Value.Points.Select(p => p.Order));
		Assert.Equal(0.3f, result.Value.Points[0].X);
		Assert.All(result.Value.Points, p => Assert.Null(p.NadeEntryId));
		Assert.Empty(dbContext.NadeEntries);
		Assert.Equal(2, dbContext.TacticPoints.Count());
		Assert.Equal(["tactics"], notifier.Topics);
	}

	[Fact]
	public async Task Should_add_new_library_entries_with_throw_pins()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = CreateHandler(dbContext, notifier);

		var result = await handler.Handle(Command(addToLibrary: true, Nade(GrenadeType.Smoke, 10f)), CancellationToken.None);

		var entry = Assert.Single(dbContext.NadeEntries);
		Assert.Equal("Smoke – kacper (z demki)", entry.Title);
		Assert.Equal(0.1f, entry.ThrowX);
		Assert.Equal(0.5f, entry.LandingX);
		Assert.Equal(entry.Id, result.Value.Points.Single().NadeEntryId);
		Assert.Equal(["tactics", "nades"], notifier.Topics);
	}

	[Fact]
	public async Task Should_link_an_existing_nearby_entry_and_dedupe_within_the_import()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var existing = new NadeEntry
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Type = GrenadeType.Smoke,
			Title = "Window",
			LandingX = 0.51f,
			LandingY = 0.5f
		};
		dbContext.NadeEntries.Add(existing);
		await dbContext.SaveChangesAsync();
		var handler = CreateHandler(dbContext, new TestRealtimeNotifier());

		var command = Command(addToLibrary: true,
			Nade(GrenadeType.Smoke, 5f),
			Nade(GrenadeType.Molotov, 8f, landX: 0.2f),
			Nade(GrenadeType.Molotov, 9f, landX: 0.205f));
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.Equal(existing.Id, result.Value.Points[0].NadeEntryId);
		Assert.Equal(result.Value.Points[1].NadeEntryId, result.Value.Points[2].NadeEntryId);
		Assert.Equal(2, dbContext.NadeEntries.Count());
	}

	#endregion

	#region Private Methods

	private static ImportTacticFromDemoHandler CreateHandler(TestApplicationDbContext dbContext, TestRealtimeNotifier notifier) =>
		new(dbContext, new TestCurrentUserService(Guid.NewGuid(), "Coach"), notifier);

	private static ImportTacticFromDemoCommand Command(bool addToLibrary, params ImportedNadeInput[] grenades) =>
		new(MapName.Mirage, MapSide.T, "Mid exec", EconomyType.FullBuy, null, grenades, addToLibrary);

	private static ImportedNadeInput Nade(GrenadeType type, float seconds, float landX = 0.5f) =>
		new(type, "kacper", 0.1f, 0.2f, landX, 0.5f, seconds);

	#endregion
}
