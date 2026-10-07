#region Usings

using HarnasHub.Application.Features.MatchAnalysis.ExcludePlayer;
using HarnasHub.Application.Features.MatchAnalysis.IncludePlayer;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.ExcludePlayer;

public class ExcludePlayerHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_exclude_once_and_restore_a_player()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matchId = Guid.NewGuid();
		dbContext.MatchDemoAnalyses.Add(new MatchDemoAnalysis { Id = Guid.NewGuid(), MatchResultId = matchId, ObjectKey = "k" });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var exclude = new ExcludePlayerHandler(dbContext);
		Assert.False((await exclude.Handle(new ExcludePlayerCommand(matchId, 76561198210690651), CancellationToken.None)).IsError);
		Assert.False((await exclude.Handle(new ExcludePlayerCommand(matchId, 76561198210690651), CancellationToken.None)).IsError);

		var stored = await dbContext.MatchDemoAnalyses.AsNoTracking().SingleAsync();
		Assert.Equal([76561198210690651L], stored.ExcludedSteamIds);

		await new IncludePlayerHandler(dbContext).Handle(new IncludePlayerCommand(matchId, 76561198210690651), CancellationToken.None);
		Assert.Empty((await dbContext.MatchDemoAnalyses.AsNoTracking().SingleAsync()).ExcludedSteamIds);
	}

	[Fact]
	public async Task Should_return_not_found_when_the_match_has_no_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new ExcludePlayerHandler(dbContext).Handle(new ExcludePlayerCommand(Guid.NewGuid(), 1), CancellationToken.None);

		Assert.Equal("MatchAnalysis.TimelineNotFound", result.FirstError.Code);
	}

	#endregion
}
