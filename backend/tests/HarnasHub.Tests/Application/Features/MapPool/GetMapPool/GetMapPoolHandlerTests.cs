using HarnasHub.Application.Features.MapPool.GetMapPool;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapPool.GetMapPool;

public class GetMapPoolHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_list_every_map_even_without_games_or_status()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new GetMapPoolHandler(dbContext).Handle(new GetMapPoolQuery(), CancellationToken.None);

		Assert.Equal(Enum.GetValues<MapName>().Length, result.Value.Count);
		Assert.All(result.Value, m =>
		{
			Assert.Null(m.Status);
			Assert.Null(m.WinRatePercentage);
			Assert.Empty(m.RecentForm);
		});
	}

	[Fact]
	public async Task Should_count_the_record_leniently_by_map_name_with_newest_form_first()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var now = DateTime.UtcNow;
		dbContext.MatchResults.AddRange(
			Match("Mirage", 13, 5, now.AddDays(-4)),
			Match(" mirage ", 10, 13, now.AddDays(-3)),
			Match("MIRAGE", 12, 12, now.AddDays(-2)),
			Match("Mirage", 13, 11, now.AddDays(-1)),
			Match("Overpass", 13, 0, now.AddDays(-1)),
			Match(null, 13, 0, now));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetMapPoolHandler(dbContext).Handle(new GetMapPoolQuery(), CancellationToken.None);

		var mirage = result.Value.Single(m => m.MapName == "Mirage");
		Assert.Equal((2, 1, 1), (mirage.Wins, mirage.Losses, mirage.Draws));
		Assert.Equal(50, mirage.WinRatePercentage);
		Assert.Equal(["W", "D", "L", "W"], mirage.RecentForm);
		Assert.Equal(now.AddDays(-1), mirage.LastPlayedAtUtc);
		Assert.Equal(4, result.Value.Sum(m => m.Wins + m.Losses + m.Draws));
	}

	[Fact]
	public async Task Should_filter_by_category_when_asked()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var now = DateTime.UtcNow;
		dbContext.MatchResults.AddRange(
			Match("Nuke", 13, 5, now, MatchCategory.League),
			Match("Nuke", 5, 13, now, MatchCategory.Scrimmage));
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetMapPoolHandler(dbContext).Handle(new GetMapPoolQuery(MatchCategory.League), CancellationToken.None);

		var nuke = result.Value.Single(m => m.MapName == "Nuke");
		Assert.Equal((1, 0), (nuke.Wins, nuke.Losses));
	}

	[Fact]
	public async Task Should_order_by_status_with_bans_last_and_include_note_and_tactic_count()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.MapPoolEntries.AddRange(
			Entry(MapName.Dust2, MapPoolStatus.Ban),
			Entry(MapName.Nuke, MapPoolStatus.Core, "Najlepsza mapa"),
			Entry(MapName.Anubis, MapPoolStatus.Learning));
		dbContext.Tactics.Add(new Tactic
		{
			Id = Guid.NewGuid(),
			Name = "Szybkie B",
			MapName = MapName.Nuke,
			Side = MapSide.T,
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var result = await new GetMapPoolHandler(dbContext).Handle(new GetMapPoolQuery(), CancellationToken.None);

		Assert.Equal("Nuke", result.Value[0].MapName);
		Assert.Equal("Core", result.Value[0].Status);
		Assert.Equal("Najlepsza mapa", result.Value[0].Note);
		Assert.Equal(1, result.Value[0].TacticCount);
		Assert.Equal("Anubis", result.Value[1].MapName);
		Assert.Equal("Dust2", result.Value[^1].MapName);
	}

	#endregion

	#region Private Methods

	private static MatchResult Match(string? map, int ourScore, int opponentScore, DateTime playedAtUtc, MatchCategory category = MatchCategory.Scrimmage) => new()
	{
		Id = Guid.NewGuid(),
		Opponent = "Team X",
		OurScore = ourScore,
		OpponentScore = opponentScore,
		MapName = map,
		Category = category,
		PlayedAtUtc = playedAtUtc,
		CreatedByUserId = Guid.NewGuid(),
		CreatedAtUtc = playedAtUtc
	};

	private static MapPoolEntry Entry(MapName map, MapPoolStatus status, string? note = null) => new()
	{
		Id = Guid.NewGuid(),
		MapName = map,
		Status = status,
		Note = note,
		UpdatedByUserId = Guid.NewGuid(),
		UpdatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
