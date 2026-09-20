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
		var handler = new AddResultHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());

		var command = new AddResultCommand(
			"Team X", 16, 10, "Mirage", null, null, DateTime.UtcNow, MatchCategory.Scrimmage, null, null);

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

		var handler = new AddResultHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());
		var command = new AddResultCommand(
			"Team X", 16, 10, "Mirage", null, null, DateTime.UtcNow, MatchCategory.Tournament, tournament.Id, null);

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

		var handler = new AddResultHandler(dbContext, new TestCurrentUserService(_userId), new TestRealtimeNotifier());
		var command = new AddResultCommand(
			"Team X", 16, 10, "Mirage", null, null, DateTime.UtcNow, MatchCategory.League, null, league.Id);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("ESEA", result.Value.LeagueName);
		Assert.Equal("2026 Wiosna", result.Value.LeagueSeason);
		Assert.Equal(LeagueType.Division1, result.Value.LeagueType);
	}

	#endregion
}
