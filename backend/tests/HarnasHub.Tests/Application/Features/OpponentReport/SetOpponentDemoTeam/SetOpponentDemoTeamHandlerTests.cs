#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.OpponentReport.SetOpponentDemoTeam;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentDemoRows;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.SetOpponentDemoTeam;

public class SetOpponentDemoTeamHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_mark_the_picked_team_and_extract_facts_from_the_stored_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var row = Row();
		dbContext.OpponentDemoAnalyses.Add(row);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage();
		await MatchTimelineStorage.SaveAsync(storage, row.TimelineObjectKey, Timeline([Round(1, MapSide.T, MapSide.T)]), 3, CancellationToken.None);

		var result = await Handler(dbContext, storage).Handle(new SetOpponentDemoTeamCommand(row.Id, "B"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("B", result.Value.OpponentTeam);
		Assert.True(result.Value.TeamResolved);
		Assert.Equal(Others, dbContext.OpponentDemoAnalyses.Single().OpponentSteamIds);
	}

	[Fact]
	public async Task Should_report_a_missing_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var row = Row();
		dbContext.OpponentDemoAnalyses.Add(row);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await Handler(dbContext, new TestFileStorage()).Handle(new SetOpponentDemoTeamCommand(row.Id, "A"), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.TimelineUnavailable.Code, result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFileStorage()).Handle(new SetOpponentDemoTeamCommand(Guid.NewGuid(), "A"), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.NotFound.Code, result.FirstError.Code);
	}

	[Theory]
	[InlineData("A", true)]
	[InlineData("B", true)]
	[InlineData("C", false)]
	[InlineData("", false)]
	public void Should_accept_only_team_a_or_b(string team, bool valid)
	{
		Assert.Equal(valid, new SetOpponentDemoTeamCommandValidator().Validate(new SetOpponentDemoTeamCommand(Guid.NewGuid(), team)).IsValid);
	}

	#endregion

	#region Private Methods

	private static SetOpponentDemoTeamHandler Handler(TestApplicationDbContext dbContext, TestFileStorage storage) =>
		new(dbContext, storage, TestTeamNotifications.Create(dbContext, storage), NullLogger<SetOpponentDemoTeamHandler>.Instance);

	#endregion
}
