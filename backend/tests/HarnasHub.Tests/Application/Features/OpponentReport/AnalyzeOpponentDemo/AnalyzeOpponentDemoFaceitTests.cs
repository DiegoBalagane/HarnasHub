#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.AnalyzeOpponentDemo;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.AnalyzeOpponentDemo;

public class AnalyzeOpponentDemoFaceitTests
{
	#region Private Fields

	private const string DemoKey = "demos/0123456789abcdef0123456789abcdef";
	private const string MatchId = "1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e";

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_resolve_the_opponent_side_and_match_from_the_faceit_room_named_by_the_file()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var client = new TestFaceitClient();
		client.Matches[MatchId] = new FaceitMatchInfo(MatchId,
		[
			new FaceitFactionInfo("f1", "Team X", Them.Select(id => new FaceitPlayerRef($"p{id}", $"n{id}", id.ToString(), 5)).ToList()),
			new FaceitFactionInfo("f2", "team_Someone", Others.Select(id => new FaceitPlayerRef($"p{id}", $"n{id}", id.ToString(), 5)).ToList())
		])
		{ StartedAtUtc = new DateTime(2026, 9, 30, 20, 0, 0, DateTimeKind.Utc) };

		var result = await Handler(dbContext, client)
			.Handle(new AnalyzeOpponentDemoCommand("Team X", DemoKey, $"{MatchId}-1-2.dem"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.True(result.Value.TeamResolved);
		Assert.Equal("A", result.Value.OpponentTeam);
		var row = Assert.Single(dbContext.OpponentDemoAnalyses);
		Assert.Equal((MatchId, (int?)2), (row.FaceitMatchId, row.FaceitMapNumber));
		Assert.Equal(new DateTime(2026, 9, 30, 20, 0, 0, DateTimeKind.Utc), row.PlayedAtUtc);
	}

	[Fact]
	public async Task Should_leave_the_side_to_the_coach_without_faceit()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFaceitClient(isConfigured: false))
			.Handle(new AnalyzeOpponentDemoCommand("Team X", DemoKey, $"{MatchId}-1-2.dem"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.False(result.Value.TeamResolved);
		Assert.Null(Assert.Single(dbContext.OpponentDemoAnalyses).FaceitMatchId);
	}

	#endregion

	#region Private Methods

	private static AnalyzeOpponentDemoHandler Handler(TestApplicationDbContext dbContext, TestFaceitClient client) =>
		new(dbContext, new TestFileStorage(), new TestDemoParser(timeline: Timeline([Round(1, MapSide.T, MapSide.T)])),
			new TestCurrentUserService(Guid.NewGuid()), TestFaceitLookup.Create(dbContext, client), NullLogger<AnalyzeOpponentDemoHandler>.Instance);

	#endregion
}
