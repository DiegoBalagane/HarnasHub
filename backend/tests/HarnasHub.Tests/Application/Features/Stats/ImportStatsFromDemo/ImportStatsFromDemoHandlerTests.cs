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
		var parser = new TestDemoParser(new DemoParseResult(0, [new DemoPlayerStats(1, "Bot", 0, 0, 0, 0, 0)]));
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
		var parsed = new DemoParseResult(10, [new DemoPlayerStats(76561198012345678, "s1mple", 4, 3, 1, 2, 500)]);
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

		var parsed = new DemoParseResult(5, [new DemoPlayerStats(76561198012345678, "s1mple", 5, 5, 0, 0, 250)]);
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
		var parsed = new DemoParseResult(5, [new DemoPlayerStats(999, "Losowy gracz", 1, 1, 0, 0, 50)]);
		var handler = new ImportStatsFromDemoHandler(new TestDemoParser(parsed), dbContext);

		var result = await handler.Handle(new ImportStatsFromDemoCommand(Stream.Null), CancellationToken.None);

		var player = Assert.Single(result.Value.Players);
		Assert.Null(player.MatchedUserId);
		Assert.Null(player.MatchedDisplayName);
	}

	#endregion
}
