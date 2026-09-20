using HarnasHub.Application.Features.Nades.UpdateNadePosition;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Nades.UpdateNadePosition;

public class UpdateNadePositionHandlerTests
{
	#region Private Fields

	private readonly Guid _authorId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_set_the_pin_and_notify_when_the_author_places_it()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var nadeId = await SeedNadeAsync(dbContext);
		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateNadePositionHandler(dbContext, new TestCurrentUserService(_authorId), notifier);

		var result = await handler.Handle(new UpdateNadePositionCommand(nadeId, 0.3f, 0.7f), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(0.3f, result.Value.LandingX);
		Assert.Equal(0.7f, result.Value.LandingY);

		var stored = await dbContext.NadeEntries.SingleAsync();
		Assert.Equal(0.3f, stored.LandingX);
		Assert.Equal(0.7f, stored.LandingY);
		Assert.Equal(["nades"], notifier.Topics);
	}

	[Fact]
	public async Task Should_allow_a_manager_to_move_someone_elses_pin()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var nadeId = await SeedNadeAsync(dbContext);
		var manager = new TestCurrentUserService(Guid.NewGuid(), role: "Manager");
		var handler = new UpdateNadePositionHandler(dbContext, manager, new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateNadePositionCommand(nadeId, 0.1f, 0.2f), CancellationToken.None);

		Assert.False(result.IsError);
	}

	[Fact]
	public async Task Should_allow_the_coach_to_move_someone_elses_pin()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var nadeId = await SeedNadeAsync(dbContext);
		var coach = new TestCurrentUserService(Guid.NewGuid(), isCoach: true);
		var handler = new UpdateNadePositionHandler(dbContext, coach, new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateNadePositionCommand(nadeId, 0.1f, 0.2f), CancellationToken.None);

		Assert.False(result.IsError);
	}

	[Fact]
	public async Task Should_reject_a_teammate_who_is_neither_the_author_nor_coach_or_manager()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var nadeId = await SeedNadeAsync(dbContext);
		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateNadePositionHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), notifier);

		var result = await handler.Handle(new UpdateNadePositionCommand(nadeId, 0.1f, 0.2f), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Nades.NotYourEntry", result.FirstError.Code);
		Assert.Empty(notifier.Topics);

		var stored = await dbContext.NadeEntries.SingleAsync();
		Assert.Null(stored.LandingX);
	}

	[Fact]
	public async Task Should_clear_an_existing_pin_when_both_coordinates_are_null()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var nadeId = await SeedNadeAsync(dbContext, landingX: 0.4f, landingY: 0.4f);
		var handler = new UpdateNadePositionHandler(dbContext, new TestCurrentUserService(_authorId), new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateNadePositionCommand(nadeId, null, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.LandingX);
		Assert.Null(result.Value.LandingY);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_nade()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateNadePositionHandler(dbContext, new TestCurrentUserService(_authorId), notifier);

		var result = await handler.Handle(new UpdateNadePositionCommand(Guid.NewGuid(), 0.5f, 0.5f), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Nades.NotFound", result.FirstError.Code);
		Assert.Empty(notifier.Topics);
	}

	#endregion

	#region Private Methods

	private async Task<Guid> SeedNadeAsync(TestApplicationDbContext dbContext, float? landingX = null, float? landingY = null)
	{
		var nade = new NadeEntry
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Type = GrenadeType.Smoke,
			Title = "Mid smoke",
			CreatedByUserId = _authorId,
			CreatedAtUtc = DateTime.UtcNow,
			LandingX = landingX,
			LandingY = landingY
		};

		dbContext.NadeEntries.Add(nade);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		return nade.Id;
	}

	#endregion
}
