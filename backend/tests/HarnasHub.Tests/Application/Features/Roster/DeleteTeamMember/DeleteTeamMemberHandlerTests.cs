using HarnasHub.Application.Features.Roster.DeleteTeamMember;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;
using TaskItem = HarnasHub.Core.Entities.TaskItem;

namespace HarnasHub.Tests.Application.Features.Roster.DeleteTeamMember;

public class DeleteTeamMemberHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_account_and_only_his_own_personal_data()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		var otherUserId = Guid.NewGuid();

		dbContext.Users.Add(CreateUser(userId, "Zenek"));
		dbContext.Availabilities.Add(new HarnasHub.Core.Entities.Availability { Id = Guid.NewGuid(), EventId = Guid.NewGuid(), UserId = userId, Status = AvailabilityStatus.Available, UpdatedAtUtc = DateTime.UtcNow });
		dbContext.MapPositionAssignments.Add(new MapPositionAssignment { Id = Guid.NewGuid(), MapName = MapName.Mirage, Side = MapSide.T, UserId = userId, X = 0.5f, Y = 0.5f, UpdatedAtUtc = DateTime.UtcNow });
		dbContext.PlayerAvailabilityDays.Add(new PlayerAvailabilityDay { Id = Guid.NewGuid(), UserId = userId, Date = DateOnly.FromDateTime(DateTime.UtcNow), Status = DayAvailabilityStatus.Available, UpdatedAtUtc = DateTime.UtcNow });
		dbContext.Vacations.Add(new Vacation { Id = Guid.NewGuid(), UserId = userId, StartDate = DateOnly.FromDateTime(DateTime.UtcNow), EndDate = DateOnly.FromDateTime(DateTime.UtcNow), CreatedAtUtc = DateTime.UtcNow });
		dbContext.UserSecondaryTeamRoles.Add(new UserSecondaryTeamRole { Id = Guid.NewGuid(), UserId = userId, TeamRole = TeamRole.AWPer });
		dbContext.Tasks.Add(new TaskItem { Id = Guid.NewGuid(), Title = "Dla niego", AssignedToUserId = userId, AssignedByUserId = otherUserId, Status = TaskItemStatus.Todo, CreatedAtUtc = DateTime.UtcNow });

		var taskHeAssigned = new TaskItem { Id = Guid.NewGuid(), Title = "Przez niego", AssignedToUserId = otherUserId, AssignedByUserId = userId, Status = TaskItemStatus.Todo, CreatedAtUtc = DateTime.UtcNow };
		dbContext.Tasks.Add(taskHeAssigned);
		var nade = new NadeEntry { Id = Guid.NewGuid(), MapName = MapName.Mirage, Type = GrenadeType.Smoke, Title = "Jego granat", CreatedByUserId = userId, CreatedAtUtc = DateTime.UtcNow };
		dbContext.NadeEntries.Add(nade);
		var tactic = new Tactic { Id = Guid.NewGuid(), MapName = MapName.Mirage, Side = MapSide.T, Name = "Jego taktyka", Economy = EconomyType.Eco, CreatedByUserId = userId, CreatedAtUtc = DateTime.UtcNow };
		dbContext.Tactics.Add(tactic);
		var match = new MatchResult { Id = Guid.NewGuid(), Opponent = "Rival", CreatedByUserId = userId, PlayedAtUtc = DateTime.UtcNow, CreatedAtUtc = DateTime.UtcNow };
		dbContext.MatchResults.Add(match);

		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new DeleteTeamMemberHandler(dbContext, new TestCurrentUserService(otherUserId, "Manager"), notifier);

		var result = await handler.Handle(new DeleteTeamMemberCommand(userId), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId));
		Assert.Empty(await dbContext.Availabilities.ToListAsync());
		Assert.Empty(await dbContext.MapPositionAssignments.ToListAsync());
		Assert.Empty(await dbContext.PlayerAvailabilityDays.ToListAsync());
		Assert.Empty(await dbContext.Vacations.ToListAsync());
		Assert.Empty(await dbContext.UserSecondaryTeamRoles.ToListAsync());
		Assert.Null(await dbContext.Tasks.FirstOrDefaultAsync(t => t.Title == "Dla niego"));

		// Team artifacts survive with a now-dangling creator/assigner reference.
		Assert.NotNull(await dbContext.Tasks.FirstOrDefaultAsync(t => t.Id == taskHeAssigned.Id));
		Assert.NotNull(await dbContext.NadeEntries.FirstOrDefaultAsync(n => n.Id == nade.Id));
		Assert.NotNull(await dbContext.Tactics.FirstOrDefaultAsync(t => t.Id == tactic.Id));
		Assert.NotNull(await dbContext.MatchResults.FirstOrDefaultAsync(m => m.Id == match.Id));

		Assert.Equal(["roster", "availability-week", "map-strategy", "tasks", "dashboard"], notifier.Topics);
	}

	[Fact]
	public async Task Should_reject_deleting_your_own_account()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, "Zenek"));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new DeleteTeamMemberHandler(dbContext, new TestCurrentUserService(userId, "Manager"), new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteTeamMemberCommand(userId), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.CannotDeleteOwnAccount", result.FirstError.Code);
		Assert.NotNull(await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId));
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteTeamMemberHandler(dbContext, new TestCurrentUserService(Guid.NewGuid(), "Manager"), new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteTeamMemberCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id, string displayName) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = displayName,
		AccessLevel = AccessLevel.Player,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
