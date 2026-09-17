using HarnasHub.Application.Features.Roster.UpdateTeamRole;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.UpdateTeamRole;

public class UpdateTeamRoleHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_assign_the_team_role_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, teamRole: null));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateTeamRoleHandler(dbContext, notifier);

		var result = await handler.Handle(new UpdateTeamRoleCommand(_userId, TeamRole.AWPer), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(nameof(TeamRole.AWPer), result.Value.TeamRole);
		Assert.Equal(TeamRole.AWPer, (await dbContext.Users.SingleAsync()).TeamRole);
		Assert.Equal(["roster"], notifier.Topics);
	}

	[Fact]
	public async Task Should_clear_the_team_role_when_none_is_given()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(CreateUser(_userId, TeamRole.IGL));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new UpdateTeamRoleHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new UpdateTeamRoleCommand(_userId, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.TeamRole);
		Assert.Null((await dbContext.Users.SingleAsync()).TeamRole);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_user_does_not_exist()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateTeamRoleHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(
			new UpdateTeamRoleCommand(Guid.NewGuid(), TeamRole.Support),
			CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Roster.UserNotFound", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private static User CreateUser(Guid id, TeamRole? teamRole) => new()
	{
		Id = id,
		DiscordId = id.ToString("N"),
		DisplayName = "Harnas",
		AccessLevel = AccessLevel.Player,
		TeamRole = teamRole,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
