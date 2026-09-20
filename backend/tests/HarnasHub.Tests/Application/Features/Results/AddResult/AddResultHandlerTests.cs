using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.AddResult;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.AddResult;

public class AddResultHandlerTests
{
	#region Private Fields

	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_persist_a_scrimmage_with_no_grouping()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = Handler(dbContext);

		var command = Command();

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(MatchCategory.Scrimmage, result.Value.Category);
		Assert.Null(result.Value.TournamentName);
		Assert.Null(result.Value.LeagueName);
	}

	[Fact]
	public async Task Should_resolve_the_tournament_name_for_a_tournament_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var tournament = new Tournament { Id = Guid.NewGuid(), Name = "Blast Q1", CreatedByUserId = _userId, CreatedAtUtc = DateTime.UtcNow };
		dbContext.Tournaments.Add(tournament);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = Handler(dbContext);
		var command = Command(category: MatchCategory.Tournament, tournamentId: tournament.Id);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(tournament.Id, result.Value.TournamentId);
		Assert.Equal("Blast Q1", result.Value.TournamentName);

		var stored = dbContext.MatchResults.Single();
		Assert.Equal(tournament.Id, stored.TournamentId);
		Assert.Equal(MatchCategory.Tournament, stored.Category);
	}

	[Fact]
	public async Task Should_resolve_the_league_name_season_and_type_for_a_league_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var league = new League
		{
			Id = Guid.NewGuid(),
			Name = "ESEA",
			Season = "2026 Wiosna",
			Type = LeagueType.Division1,
			CreatedByUserId = _userId,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Leagues.Add(league);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = Handler(dbContext);
		var command = Command(category: MatchCategory.League, leagueId: league.Id);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("ESEA", result.Value.LeagueName);
		Assert.Equal("2026 Wiosna", result.Value.LeagueSeason);
		Assert.Equal(LeagueType.Division1, result.Value.LeagueType);
	}

	[Fact]
	public async Task Should_require_a_score_when_there_is_neither_a_demo_nor_a_manual_value()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = Handler(dbContext);

		var result = await handler.Handle(Command(ourScore: null, opponentScore: null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.ScoreRequired", result.FirstError.Code);
		Assert.Empty(dbContext.MatchResults);
	}

	[Fact]
	public async Task Should_reject_a_demo_the_parser_cannot_read()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = Handler(dbContext, new TestDemoParser(throwOnParse: new InvalidDataException("corrupt")));

		var result = await handler.Handle(Command(demoStream: Stream.Null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.InvalidDemoFile", result.FirstError.Code);
		Assert.Empty(dbContext.MatchResults);
	}

	[Fact]
	public async Task Should_compute_the_score_and_map_from_an_attached_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await AddRosterMemberAsync(dbContext, "76561198012345678");

		// Three rounds on T, then the same roster on CT after the swap — two wins in each half.
		var parsed = ParseResult(MapName.Mirage,
		[
			Round(MapSide.T, [76561198012345678], [999]),
			Round(MapSide.T, [76561198012345678], [999]),
			Round(MapSide.CT, [76561198012345678], [999]),
			Round(MapSide.CT, [999], [76561198012345678]),
			Round(MapSide.CT, [999], [76561198012345678]),
			Round(MapSide.T, [999], [76561198012345678])
		]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(
			Command(ourScore: null, opponentScore: null, mapName: null, demoStream: Stream.Null),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(4, result.Value.OurScore);
		Assert.Equal(2, result.Value.OpponentScore);
		Assert.Equal("Mirage", result.Value.MapName);
	}

	[Fact]
	public async Task Should_let_the_demo_override_a_manually_entered_score()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		await AddRosterMemberAsync(dbContext, "76561198012345678");

		var parsed = ParseResult(MapName.Inferno, [Round(MapSide.T, [76561198012345678], [999])]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(
			Command(ourScore: 13, opponentScore: 7, mapName: "Mirage", demoStream: Stream.Null),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(1, result.Value.OurScore);
		Assert.Equal(0, result.Value.OpponentScore);
		Assert.Equal("Inferno", result.Value.MapName);
	}

	[Fact]
	public async Task Should_fall_back_to_the_manual_score_when_no_roster_member_plays_in_the_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parsed = ParseResult(null, [Round(MapSide.T, [998], [999])]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(
			Command(ourScore: 13, opponentScore: 7, mapName: "Mirage", demoStream: Stream.Null),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(13, result.Value.OurScore);
		Assert.Equal(7, result.Value.OpponentScore);
		Assert.Equal("Mirage", result.Value.MapName);
	}

	[Fact]
	public async Task Should_require_a_score_when_the_demo_cannot_be_attributed_and_none_was_entered()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parsed = ParseResult(null, [Round(MapSide.T, [998], [999])]);
		var handler = Handler(dbContext, new TestDemoParser(parsed));

		var result = await handler.Handle(
			Command(ourScore: null, opponentScore: null, demoStream: Stream.Null),
			CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.ScoreRequired", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private AddResultHandler Handler(IApplicationDbContext dbContext, TestDemoParser? demoParser = null) =>
		new(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier(), demoParser ?? new TestDemoParser());

	private static AddResultCommand Command(
		MatchCategory category = MatchCategory.Scrimmage,
		Guid? tournamentId = null,
		Guid? leagueId = null,
		int? ourScore = 16,
		int? opponentScore = 10,
		string? mapName = "Mirage",
		Stream? demoStream = null) =>
		new("Team X", ourScore, opponentScore, mapName, null, null, DateTime.UtcNow, category, tournamentId, leagueId, demoStream);

	private static DemoParseResult ParseResult(MapName? mapName, IReadOnlyList<DemoRoundResult> rounds) =>
		new(rounds.Count, mapName, [], rounds);

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
