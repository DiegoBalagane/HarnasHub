using HarnasHub.Application.Features.Leagues.CreateLeague;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Leagues.CreateLeague;

public class CreateLeagueHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_persist_the_league_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new CreateLeagueHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), notifier);

		var command = new CreateLeagueCommand("ESEA", "2026 Wiosna", LeagueType.Division1);
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("ESEA", result.Value.Name);
		Assert.Equal(LeagueType.Division1, result.Value.Type);

		var stored = dbContext.Leagues.Single();
		Assert.Equal("2026 Wiosna", stored.Season);
		Assert.Equal(["leagues"], notifier.Topics);
	}

	#endregion
}
