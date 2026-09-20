using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.AddResult;
using HarnasHub.Application.Features.Results.Shared;
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
	public async Task Should_require_a_score_when_none_was_entered()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = Handler(dbContext);

		var result = await handler.Handle(Command(ourScore: null, opponentScore: null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.ScoreRequired", result.FirstError.Code);
		Assert.Empty(dbContext.MatchResults);
	}

	[Fact]
	public async Task Should_import_a_stat_line_for_every_roster_member_matched_in_an_analysed_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var matched = await AddRosterMemberAsync(dbContext, "76561198012345678");

		var demoPlayers = new[]
		{
			Player("76561198012345678", "shadow", kills: 20, deaths: 10, assists: 5, headshots: 8, damage: 1500, kastRounds: 1),
			Player("999", "unmatched-opponent", kills: 10, deaths: 20)
		};
		var handler = Handler(dbContext);

		var result = await handler.Handle(
			Command(demoRoundsPlayed: 1, demoPlayers: demoPlayers),
			CancellationToken.None);

		Assert.False(result.IsError);

		var stat = Assert.Single(dbContext.PlayerMatchStats);
		Assert.Equal(matched.Id, stat.UserId);
		Assert.Equal(result.Value.Id, stat.MatchResultId);
		Assert.Equal(20, stat.Kills);
		Assert.Equal(10, stat.Deaths);
		Assert.NotNull(stat.KastPercentage);
	}

	[Fact]
	public async Task Should_not_import_any_stats_when_nobody_in_the_analysed_demo_matches_the_roster()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var demoPlayers = new[] { Player("999", "opponent", kills: 10, deaths: 5) };
		var handler = Handler(dbContext);

		var result = await handler.Handle(
			Command(ourScore: 13, opponentScore: 7, demoRoundsPlayed: 1, demoPlayers: demoPlayers),
			CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.PlayerMatchStats);
	}

	#endregion

	#region Private Methods

	private AddResultHandler Handler(IApplicationDbContext dbContext) =>
		new(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

	private static AddResultCommand Command(
		MatchCategory category = MatchCategory.Scrimmage,
		Guid? tournamentId = null,
		Guid? leagueId = null,
		int? ourScore = 16,
		int? opponentScore = 10,
		string? mapName = "Mirage",
		int? demoRoundsPlayed = null,
		IReadOnlyList<AnalyzedDemoPlayerDto>? demoPlayers = null) =>
		new("Team X", ourScore, opponentScore, mapName, null, null, DateTime.UtcNow, category, tournamentId, leagueId, demoRoundsPlayed, demoPlayers);

	private static async Task<User> AddRosterMemberAsync(IApplicationDbContext dbContext, string steamId64)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "1",
			DisplayName = "Kacper",
			AccessLevel = AccessLevel.Player,
			SteamId64 = steamId64,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		return user;
	}

	private static AnalyzedDemoPlayerDto Player(
		string steamId64,
		string name,
		int kills = 0,
		int deaths = 0,
		int assists = 0,
		int headshots = 0,
		int damage = 0,
		int entryKills = 0,
		int entryDeaths = 0,
		int kastRounds = 0,
		int utilityDamage = 0,
		int flashAssists = 0) =>
		new(steamId64, name, kills, deaths, assists, headshots, damage, entryKills, entryDeaths, kastRounds, utilityDamage, flashAssists, 0, 0, 0, 0);

	#endregion
}
