using HarnasHub.Application.Features.Availability.UpdateVacation;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Availability.UpdateVacation;

public class UpdateVacationHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_owners_vacation_in_place()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var vacation = new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			StartDate = new DateOnly(2026, 9, 20),
			EndDate = new DateOnly(2026, 9, 22),
			Reason = "Stary powód",
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Vacations.Add(vacation);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateVacationHandler(dbContext, new TestCurrentUserService(userId), new TestRealtimeNotifier());

		var command = new UpdateVacationCommand(vacation.Id, new DateOnly(2026, 9, 25), new DateOnly(2026, 9, 27), "Nowy powód");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.Vacations.SingleAsync();
		Assert.Equal(new DateOnly(2026, 9, 25), stored.StartDate);
		Assert.Equal(new DateOnly(2026, 9, 27), stored.EndDate);
		Assert.Equal("Nowy powód", stored.Reason);
	}

	[Fact]
	public async Task Should_reject_editing_someone_elses_vacation_for_a_regular_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var ownerId = Guid.NewGuid();
		var vacation = new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = ownerId,
			StartDate = new DateOnly(2026, 9, 20),
			EndDate = new DateOnly(2026, 9, 22),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Vacations.Add(vacation);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateVacationHandler(
			dbContext,
			new TestCurrentUserService(Guid.NewGuid(), "Player"),
			new TestRealtimeNotifier());

		var command = new UpdateVacationCommand(vacation.Id, new DateOnly(2026, 9, 25), new DateOnly(2026, 9, 27), null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal(new DateOnly(2026, 9, 20), (await dbContext.Vacations.SingleAsync()).StartDate);
	}

	[Fact]
	public async Task Should_allow_a_manager_to_edit_someone_elses_vacation()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var ownerId = Guid.NewGuid();
		var vacation = new Vacation
		{
			Id = Guid.NewGuid(),
			UserId = ownerId,
			StartDate = new DateOnly(2026, 9, 20),
			EndDate = new DateOnly(2026, 9, 22),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Vacations.Add(vacation);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateVacationHandler(
			dbContext,
			new TestCurrentUserService(Guid.NewGuid(), "Manager"),
			new TestRealtimeNotifier());

		var command = new UpdateVacationCommand(vacation.Id, new DateOnly(2026, 9, 25), new DateOnly(2026, 9, 27), null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_vacation()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateVacationHandler(
			dbContext,
			new TestCurrentUserService(Guid.NewGuid()),
			new TestRealtimeNotifier());

		var command = new UpdateVacationCommand(Guid.NewGuid(), new DateOnly(2026, 9, 25), new DateOnly(2026, 9, 27), null);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
	}

	#endregion
}
