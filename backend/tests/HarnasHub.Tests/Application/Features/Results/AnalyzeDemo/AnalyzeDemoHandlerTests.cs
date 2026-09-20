using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.AnalyzeDemo;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.AnalyzeDemo;

public class AnalyzeDemoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_reject_a_demo_the_parser_cannot_read()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = Handler(dbContext, new TestDemoParser(throwOnParse: new InvalidDataException("corrupt")));

		var result = await handler.Handle(new AnalyzeDemoCommand(Stream.Null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.InvalidDemoFile", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_reject_a_demo_with_no_resolvable_rounds()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parsed = new DemoParseResult(0, null, [], []);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(new AnalyzeDemoCommand(Stream.Null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.InvalidDemoFile", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_score_each_team_split_independently_of_a_halftime_side_swap()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		// Team A (1001-1005) plays T for the first three rounds, then CT for the last three — two wins each half.
		var parsed = ParseResult(MapName.Mirage,
		[
			Player(1001, "alpha-1"), Player(1002, "alpha-2"), Player(2001, "bravo-1"), Player(2002, "bravo-2"),
			Round(MapSide.T, [1001, 1002], [2001, 2002]),
			Round(MapSide.T, [1001, 1002], [2001, 2002]),
			Round(MapSide.CT, [1001, 1002], [2001, 2002]),
			Round(MapSide.CT, [2001, 2002], [1001, 1002]),
			Round(MapSide.CT, [2001, 2002], [1001, 1002]),
			Round(MapSide.T, [2001, 2002], [1001, 1002])
		]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(new AnalyzeDemoCommand(Stream.Null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Mirage", result.Value.MapName);
		Assert.Equal(6, result.Value.RoundsPlayed);

		// Team A: T wins in rounds 1-3 (2 of 3), CT wins in rounds 4-6 (2 of 3) -> 4-2.
		Assert.Equal(4, result.Value.TeamA.OurScore);
		Assert.Equal(2, result.Value.TeamA.OpponentScore);
		Assert.Contains("alpha-1", result.Value.TeamA.PlayerNames);

		// Team B is the exact mirror.
		Assert.Equal(2, result.Value.TeamB.OurScore);
		Assert.Equal(4, result.Value.TeamB.OpponentScore);
		Assert.Contains("bravo-1", result.Value.TeamB.PlayerNames);
	}

	[Fact]
	public async Task Should_suggest_whichever_team_split_overlaps_the_roster()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await AddRosterMemberAsync(dbContext, "2001");

		var parsed = ParseResult(MapName.Mirage,
		[
			Player(1001, "alpha-1"), Player(2001, "bravo-1"),
			Round(MapSide.T, [1001], [2001])
		]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(new AnalyzeDemoCommand(Stream.Null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("B", result.Value.SuggestedTeam);
	}

	[Fact]
	public async Task Should_suggest_nothing_when_the_roster_has_no_steam_id_set()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var parsed = ParseResult(MapName.Mirage,
		[
			Player(1001, "alpha-1"), Player(2001, "bravo-1"),
			Round(MapSide.T, [1001], [2001])
		]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(new AnalyzeDemoCommand(Stream.Null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.SuggestedTeam);
	}

	#endregion

	#region Private Methods

	private static AnalyzeDemoHandler Handler(IApplicationDbContext dbContext, TestDemoParser demoParser) => new(demoParser, dbContext);

	private static DemoParseResult ParseResult(MapName? mapName, params object[] playersAndRounds)
	{
		var players = playersAndRounds.OfType<DemoPlayerStats>().ToList();
		var rounds = playersAndRounds.OfType<DemoRoundResult>().ToList();
		return new DemoParseResult(rounds.Count, mapName, players, rounds);
	}

	private static DemoPlayerStats Player(long steamId64, string name) =>
		new(steamId64, name, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, new Dictionary<int, int> { [2] = 0, [3] = 0, [4] = 0, [5] = 0 }, []);

	private static DemoRoundResult Round(MapSide winnerSide, IReadOnlyList<long> terrorists, IReadOnlyList<long> counterTerrorists) =>
		new(winnerSide, terrorists, counterTerrorists);

	private static async Task AddRosterMemberAsync(IApplicationDbContext dbContext, string steamId64)
	{
		dbContext.Users.Add(new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "1",
			DisplayName = "Kacper",
			AccessLevel = AccessLevel.Player,
			SteamId64 = steamId64,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);
	}

	#endregion
}
