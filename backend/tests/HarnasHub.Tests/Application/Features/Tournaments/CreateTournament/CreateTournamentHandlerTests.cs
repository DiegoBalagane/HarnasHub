using HarnasHub.Application.Features.Tournaments.CreateTournament;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Tournaments.CreateTournament;

public class CreateTournamentHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_persist_the_tournament_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new CreateTournamentHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), notifier);

		var result = await handler.Handle(new CreateTournamentCommand("Blast Q1"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Blast Q1", result.Value.Name);
		Assert.Equal("Blast Q1", dbContext.Tournaments.Single().Name);
		Assert.Equal(["tournaments"], notifier.Topics);
	}

	#endregion
}
