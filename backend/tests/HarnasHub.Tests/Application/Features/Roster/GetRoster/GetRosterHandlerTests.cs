using HarnasHub.Application.Features.Roster.GetRoster;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Roster.GetRoster;

public class GetRosterHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_include_secondary_team_roles_alongside_the_primary_one()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString(),
			DisplayName = "Zenek",
			Role = UserRole.Player,
			TeamRole = TeamRole.Rifler,
			CreatedAtUtc = DateTime.UtcNow
		});
		dbContext.UserSecondaryTeamRoles.Add(new UserSecondaryTeamRole
		{
			Id = Guid.NewGuid(),
			UserId = userId,
			TeamRole = TeamRole.AWPer
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetRosterHandler(dbContext);

		var result = await handler.Handle(new GetRosterQuery(), CancellationToken.None);

		var member = Assert.Single(result.Value);
		Assert.Equal(nameof(TeamRole.Rifler), member.TeamRole);
		Assert.Equal([nameof(TeamRole.AWPer)], member.SecondaryTeamRoles);
	}

	[Fact]
	public async Task Should_return_an_empty_list_when_no_secondary_roles_are_set()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var userId = Guid.NewGuid();
		dbContext.Users.Add(new User
		{
			Id = userId,
			DiscordId = userId.ToString(),
			DisplayName = "Zenek",
			Role = UserRole.Player,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetRosterHandler(dbContext);

		var result = await handler.Handle(new GetRosterQuery(), CancellationToken.None);

		Assert.Empty(Assert.Single(result.Value).SecondaryTeamRoles);
	}

	#endregion
}
