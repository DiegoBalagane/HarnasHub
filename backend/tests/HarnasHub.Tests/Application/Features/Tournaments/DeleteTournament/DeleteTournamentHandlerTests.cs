using HarnasHub.Application.Features.Tournaments.DeleteTournament;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tournaments.DeleteTournament;

public class DeleteTournamentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_tournament_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var tournament = new Tournament
		{
			Id = Guid.NewGuid(),
			Name = "IEM Katowice",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Tournaments.Add(tournament);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new DeleteTournamentHandler(dbContext, notifier);

		var result = await handler.Handle(new DeleteTournamentCommand(tournament.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.Tournaments.ToListAsync());
		Assert.Equal(["tournaments", "results"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_tournament()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteTournamentHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteTournamentCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tournaments.TournamentNotFound", result.FirstError.Code);
	}

	#endregion
}
