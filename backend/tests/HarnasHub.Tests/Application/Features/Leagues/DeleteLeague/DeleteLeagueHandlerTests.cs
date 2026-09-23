using HarnasHub.Application.Features.Leagues.DeleteLeague;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Leagues.DeleteLeague;

public class DeleteLeagueHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_league_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var league = new League
		{
			Id = Guid.NewGuid(),
			Name = "ESEA",
			Season = "2026 Wiosna",
			Type = LeagueType.Division1,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Leagues.Add(league);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new DeleteLeagueHandler(dbContext, notifier);

		var result = await handler.Handle(new DeleteLeagueCommand(league.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.Leagues.ToListAsync());
		Assert.Equal(["leagues", "results"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_league()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteLeagueHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteLeagueCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Leagues.LeagueNotFound", result.FirstError.Code);
	}

	#endregion
}
