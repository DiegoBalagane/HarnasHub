using HarnasHub.Application.Features.Results.GetResults;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.GetResults;

public class GetResultsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_leave_tournament_and_league_fields_null_for_a_scrimmage()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var creatorId = Guid.NewGuid();
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = "Team X",
			OurScore = 16,
			OpponentScore = 10,
			PlayedAtUtc = DateTime.UtcNow,
			Category = MatchCategory.Scrimmage,
			CreatedByUserId = creatorId,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetResultsHandler(dbContext);
		var result = await handler.Handle(new GetResultsQuery(), CancellationToken.None);

		var dto = Assert.Single(result.Value);
		Assert.Equal(MatchCategory.Scrimmage, dto.Category);
		Assert.Null(dto.TournamentName);
		Assert.Null(dto.LeagueName);
	}

	[Fact]
	public async Task Should_join_the_tournament_name_for_a_tournament_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var creatorId = Guid.NewGuid();
		var tournament = new Tournament { Id = Guid.NewGuid(), Name = "Blast Q1", CreatedByUserId = creatorId, CreatedAtUtc = DateTime.UtcNow };
		dbContext.Tournaments.Add(tournament);
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = "Team X",
			OurScore = 16,
			OpponentScore = 10,
			PlayedAtUtc = DateTime.UtcNow,
			Category = MatchCategory.Tournament,
			TournamentId = tournament.Id,
			CreatedByUserId = creatorId,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetResultsHandler(dbContext);
		var result = await handler.Handle(new GetResultsQuery(), CancellationToken.None);

		var dto = Assert.Single(result.Value);
		Assert.Equal(tournament.Id, dto.TournamentId);
		Assert.Equal("Blast Q1", dto.TournamentName);
		Assert.Null(dto.LeagueName);
	}

	[Fact]
	public async Task Should_join_the_league_name_season_and_type_for_a_league_result()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var creatorId = Guid.NewGuid();
		var league = new League
		{
			Id = Guid.NewGuid(),
			Name = "ESEA",
			Season = "2026 Wiosna",
			Type = LeagueType.Online,
			CreatedByUserId = creatorId,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Leagues.Add(league);
		dbContext.MatchResults.Add(new MatchResult
		{
			Id = Guid.NewGuid(),
			Opponent = "Team X",
			OurScore = 16,
			OpponentScore = 10,
			PlayedAtUtc = DateTime.UtcNow,
			Category = MatchCategory.League,
			LeagueId = league.Id,
			CreatedByUserId = creatorId,
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetResultsHandler(dbContext);
		var result = await handler.Handle(new GetResultsQuery(), CancellationToken.None);

		var dto = Assert.Single(result.Value);
		Assert.Equal("ESEA", dto.LeagueName);
		Assert.Equal("2026 Wiosna", dto.LeagueSeason);
		Assert.Equal(LeagueType.Online, dto.LeagueType);
	}

	#endregion
}
