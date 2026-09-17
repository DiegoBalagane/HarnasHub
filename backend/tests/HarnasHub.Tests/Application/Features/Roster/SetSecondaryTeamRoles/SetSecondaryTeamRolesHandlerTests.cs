using HarnasHub.Application.Features.Roster.SetSecondaryTeamRoles;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.SetSecondaryTeamRoles;

public class SetSecondaryTeamRolesHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_assign_the_requested_secondary_roles()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, TeamRole.Rifler));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetSecondaryTeamRolesHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(
			new SetSecondaryTeamRolesCommand(userId, [TeamRole.AWPer, TeamRole.IGL]),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal([nameof(TeamRole.AWPer), nameof(TeamRole.IGL)], result.Value.SecondaryTeamRoles);

		var stored = await dbContext.UserSecondaryTeamRoles.Where(r => r.UserId == userId).ToListAsync();
		Assert.Equal(2, stored.Count);
	}

	[Fact]
	public async Task Should_replace_the_previous_set_rather_than_append()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, TeamRole.Rifler));
		dbContext.UserSecondaryTeamRoles.Add(new UserSecondaryTeamRole
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			TeamRole = TeamRole.Support
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetSecondaryTeamRolesHandler(dbContext, new TestRealtimeNotifier());

		await handler.Handle(new SetSecondaryTeamRolesCommand(userId, [TeamRole.AWPer]), CancellationToken.None);

		var stored = await dbContext.UserSecondaryTeamRoles.Where(r => r.UserId == userId).ToListAsync();
		Assert.Equal([TeamRole.AWPer], stored.Select(r => r.TeamRole));
	}

	[Fact]
	public async Task Should_reject_a_secondary_role_matching_the_primary_role()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(CreateUser(userId, TeamRole.AWPer));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new SetSecondaryTeamRolesHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(
			new SetSecondaryTeamRolesCommand(userId, [TeamRole.AWPer]),
			CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.SecondaryRoleMatchesPrimary", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_user()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new SetSecondaryTeamRolesHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(
			new SetSecondaryTeamRolesCommand(Guid.NewGuid(), [TeamRole.AWPer]),
			CancellationToken.None);

		Assert.True(result.IsError);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id, TeamRole? teamRole) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Zawodnik",
		AccessLevel = AccessLevel.Player,
		TeamRole = teamRole,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
