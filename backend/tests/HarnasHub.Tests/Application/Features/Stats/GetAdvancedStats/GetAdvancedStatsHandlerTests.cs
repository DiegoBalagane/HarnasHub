#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Stats.GetAdvancedStats;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.MatchAnalysis;
using HarnasHub.Tests.Application.Features.Tactics;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Stats.GetAdvancedStats;

public class GetAdvancedStatsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_fold_openings_trades_and_stat_lines_of_our_players()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		var alice = AddUser(dbContext, "Alice", 1);
		AddUser(dbContext, "Bob", 2);
		var match = await SeedAsync(dbContext, storage, "Mirage", MatchCategory.League);
		AddStat(dbContext, match.Id, alice.Id, rating: 1.30, adr: 90, utility: 100);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await CreateHandler(dbContext, storage).Handle(new GetAdvancedStatsQuery(null, null, null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(1, result.Value.MatchesAnalyzed);
		var player = Assert.Single(result.Value.Players, p => p.UserId == alice.Id);
		Assert.Equal(1, player.OpeningWonT);
		Assert.Equal(0, player.OpeningLostT);
		Assert.Equal(1, player.TradeKills);
		Assert.Equal(1.30, player.AvgRating);
		Assert.Equal(100d, player.UtilityDamagePerMatch);
		var bob = Assert.Single(result.Value.Players, p => p.Name == "p2");
		Assert.Equal(1, bob.TradedDeaths);
		Assert.Null(bob.AvgRating);
		var point = Assert.Single(result.Value.Form);
		Assert.Equal(alice.Id, point.UserId);
		Assert.Equal("Mirage", point.Map);
		var cell = Assert.Single(result.Value.MapCells);
		Assert.Equal(1, cell.Matches);
	}

	[Fact]
	public async Task Should_skip_players_hidden_from_stats()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		AddUser(dbContext, "Alice", 1).ShowInStats = false;
		await SeedAsync(dbContext, storage, "Mirage", MatchCategory.Scrimmage);

		var result = await CreateHandler(dbContext, storage).Handle(new GetAdvancedStatsQuery(null, null, null), CancellationToken.None);

		Assert.Empty(result.Value.Players);
	}

	[Fact]
	public async Task Should_filter_by_category_and_map()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var storage = new TestFileStorage();
		AddUser(dbContext, "Alice", 1);
		await SeedAsync(dbContext, storage, "Mirage", MatchCategory.League);
		await SeedAsync(dbContext, storage, "Dust2", MatchCategory.Scrimmage);
		var handler = CreateHandler(dbContext, storage);

		var byCategory = await handler.Handle(new GetAdvancedStatsQuery(MatchCategory.League, null, null), CancellationToken.None);
		var byMap = await handler.Handle(new GetAdvancedStatsQuery(null, MapName.Dust2, null), CancellationToken.None);
		var last = await handler.Handle(new GetAdvancedStatsQuery(null, null, 1), CancellationToken.None);

		Assert.Equal(1, byCategory.Value.MatchesAnalyzed);
		Assert.Equal(1, byMap.Value.MatchesAnalyzed);
		Assert.Equal(1, last.Value.MatchesAnalyzed);
	}

	[Fact]
	public async Task Should_return_an_empty_result_without_demos()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddUser(dbContext, "Alice", 1);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await CreateHandler(dbContext, new TestFileStorage()).Handle(new GetAdvancedStatsQuery(null, null, null), CancellationToken.None);

		Assert.Equal(0, result.Value.MatchesAnalyzed);
		Assert.Empty(result.Value.Players);
	}

	[Fact]
	public async Task Should_count_unreadable_timelines_as_skipped()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddUser(dbContext, "Alice", 1);
		await SeedAsync(dbContext, new TestFileStorage(), "Mirage", MatchCategory.Scrimmage);

		var broken = new TestFileStorage(throwOnOpenRead: new IOException("gone"));
		var result = await CreateHandler(dbContext, broken).Handle(new GetAdvancedStatsQuery(null, null, null), CancellationToken.None);

		Assert.Equal(0, result.Value.MatchesAnalyzed);
		Assert.Equal(1, result.Value.MatchesSkipped);
	}

	#endregion

	#region Private Methods

	private static GetAdvancedStatsHandler CreateHandler(TestApplicationDbContext dbContext, TestFileStorage storage) =>
		new(dbContext, storage, NullLogger<GetAdvancedStatsHandler>.Instance);

	private static User AddUser(TestApplicationDbContext dbContext, string name, long steamId)
	{
		var user = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = Guid.NewGuid().ToString("N"),
			DisplayName = name,
			SteamId64 = steamId.ToString(),
			ShowInStats = true,
			AccessLevel = AccessLevel.Player,
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Users.Add(user);
		return user;
	}

	private static void AddStat(TestApplicationDbContext dbContext, Guid matchId, Guid userId, double rating, double adr, int utility) =>
		dbContext.PlayerMatchStats.Add(new PlayerMatchStat
		{
			Id = Guid.NewGuid(),
			MatchResultId = matchId,
			UserId = userId,
			Rating = rating,
			Adr = adr,
			UtilityDamage = utility,
			CreatedAtUtc = DateTime.UtcNow
		});

	/// <summary>One round won by T (team A): player 3 kills 2, player 1 avenges within the trade window and wins the opening duel's mirror — here 1 opens, 3 trades 2, 1 trades back.</summary>
	private static async Task<MatchResult> SeedAsync(TestApplicationDbContext dbContext, TestFileStorage storage, string map, MatchCategory category)
	{
		var match = MatchTimelineFactory.Result();
		match.MapName = map;
		match.Category = category;
		match.PlayedAtUtc = DateTime.UtcNow.AddMinutes(dbContext.MatchResults.Count());
		dbContext.MatchResults.Add(match);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var timeline = MatchTimelineFactory.Timeline(
			[MatchTimelineFactory.Round(1, MapSide.T)],
			kills:
			[
				MatchTimelineFactory.Kill(1, killer: 3, victim: 2, isOpening: false, seconds: 5f),
				MatchTimelineFactory.Kill(1, killer: 1, victim: 3, isOpening: false, seconds: 8f)
			]);
		timeline = timeline with { Kills = [MatchTimelineFactory.Kill(1, 1, 4, isOpening: true, seconds: 2f), .. timeline.Kills] };

		await MatchTimelineAttacher.AttachAsync(dbContext, storage, match.Id, timeline, DemoTimelineSerializer.CurrentParserVersion, DemoTimelineFactory.TeamA, CancellationToken.None);
		return match;
	}

	#endregion
}
