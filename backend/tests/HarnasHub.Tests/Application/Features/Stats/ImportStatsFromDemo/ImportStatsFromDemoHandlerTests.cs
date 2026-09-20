using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Stats.ImportStatsFromDemo;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Stats.ImportStatsFromDemo;

public class ImportStatsFromDemoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_invalid_demo_file_when_the_parser_throws()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(throwOnParse: new InvalidDataException("corrupt")), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Stats.InvalidDemoFile", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_invalid_demo_file_when_no_rounds_were_parsed()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parser = new TestDemoParser(new DemoParseResult(0, null, [Player(1, "Bot")]));
		var handler = new ImportStatsFromDemoHandler(parser, dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Stats.InvalidDemoFile", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_compute_adr_and_headshot_percentage_from_raw_totals()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		// 10 rounds, 500 total damage dealt -> 50 ADR; 2 of 4 kills were headshots -> 50%.
		var parsed = new DemoParseResult(10, null, [Player(76561198012345678, "s1mple", kills: 4, deaths: 3, assists: 1, headshots: 2, damage: 500)]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		Assert.False(result.IsError);
		var player = Assert.Single(result.Value.Players);
		Assert.Equal(50.0, player.Adr);
		Assert.Equal(50.0, player.HeadshotPercentage);
		Assert.Null(player.MatchedUserId);
		Assert.Equal("s1mple", player.DemoPlayerName);
	}

	[Fact]
	public async Task Should_compute_kast_percentage_from_kast_rounds()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		// Contributed (kill/assist/survived/traded) in 18 of 24 rounds -> 75%.
		var parsed = new DemoParseResult(24, null, [Player(1, "shadow", kastRounds: 18)]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		var player = Assert.Single(result.Value.Players);
		Assert.Equal(75.0, player.KastPercentage);
	}

	[Fact]
	public async Task Should_pass_through_entry_kills_entry_deaths_multikill_rounds_utility_damage_and_flash_assists()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parsed = new DemoParseResult(20, null, [
			Player(1, "magnificull", entryKills: 4, entryDeaths: 2, utilityDamage: 120, flashAssists: 3,
				multiKillRounds: new Dictionary<int, int> { [2] = 3, [3] = 1, [4] = 0, [5] = 0 })
		]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		var player = Assert.Single(result.Value.Players);
		Assert.Equal(4, player.EntryKills);
		Assert.Equal(2, player.EntryDeaths);
		Assert.Equal(120, player.UtilityDamage);
		Assert.Equal(3, player.FlashAssists);
		Assert.Equal(3, player.MultiKill2K);
		Assert.Equal(1, player.MultiKill3K);
		Assert.Equal(0, player.MultiKill4K);
		Assert.Equal(0, player.MultiKill5K);
	}

	[Fact]
	public async Task Should_convert_death_positions_and_map_name_for_display()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parsed = new DemoParseResult(5, MapName.Mirage, [
			Player(1, "shadow", deathPositions: [new DemoDeathPosition(0.25f, 0.6f, MapSide.CT), new DemoDeathPosition(0.4f, 0.1f, MapSide.T)])
		]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		Assert.Equal("Mirage", result.Value.MapName);
		var player = Assert.Single(result.Value.Players);
		Assert.Equal(2, player.DeathPositions.Count);
		Assert.Equal("CT", player.DeathPositions[0].Side);
		Assert.Equal(0.25f, player.DeathPositions[0].X);
	}

	[Fact]
	public async Task Should_match_a_roster_member_by_steam_id_64()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var user = new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "1",
			DisplayName = "Kacper",
			InGameNickname = "s1mple",
			AccessLevel = AccessLevel.Player,
			SteamId64 = "76561198012345678",
			CreatedAtUtc = DateTime.UtcNow
		};
		dbContext.Users.Add(user);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var parsed = new DemoParseResult(5, null, [Player(76561198012345678, "s1mple", kills: 5, deaths: 5, damage: 250)]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		var player = Assert.Single(result.Value.Players);
		Assert.Equal(user.Id, player.MatchedUserId);
		Assert.Equal("s1mple", player.MatchedDisplayName);
	}

	[Fact]
	public async Task Should_leave_a_player_unmatched_when_no_roster_member_shares_their_steam_id_64()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var parsed = new DemoParseResult(5, null, [Player(999, "Losowy gracz", kills: 1, deaths: 1, damage: 50)]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		var player = Assert.Single(result.Value.Players);
		Assert.Null(player.MatchedUserId);
		Assert.Null(player.MatchedDisplayName);
	}

	#endregion

	#region Private Methods

	private static DemoPlayerStats Player(
		long steamId64,
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
		int flashAssists = 0,
		Dictionary<int, int>? multiKillRounds = null,
		List<DemoDeathPosition>? deathPositions = null) =>
		new(
			steamId64, name, kills, deaths, assists, headshots, damage,
			entryKills, entryDeaths, kastRounds, utilityDamage, flashAssists,
			multiKillRounds ?? new Dictionary<int, int> { [2] = 0, [3] = 0, [4] = 0, [5] = 0 },
			deathPositions ?? []);

	#endregion
}
