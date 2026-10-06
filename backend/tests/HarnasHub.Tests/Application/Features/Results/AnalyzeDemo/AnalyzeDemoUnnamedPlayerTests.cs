using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.AnalyzeDemo;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.AnalyzeDemo;

public class AnalyzeDemoUnnamedPlayerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_replace_a_steamid_name_with_the_roster_nickname_and_fallback()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User
		{
			Id = Guid.NewGuid(),
			DiscordId = "1",
			DisplayName = "Kacper",
			InGameNickname = "harnas",
			AccessLevel = AccessLevel.Player,
			SteamId64 = "1001",
			CreatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync();

		var stats = new Dictionary<int, int> { [2] = 0, [3] = 0, [4] = 0, [5] = 0 };
		DemoPlayerStats Player(long id, string name) => new(id, name, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, stats, []);
		var parsed = new DemoParseResult(1, MapName.Mirage,
			[Player(1001, "1001"), Player(2001, "2001"), Player(2002, "bravo")],
			[new DemoRoundResult(MapSide.T, [1001], [2001, 2002])]);
		var handler = new AnalyzeDemoHandler(new TestDemoParser(parsed), dbContext, new TestFileStorage(isConfigured: false), TestFaceitLookup.Create(dbContext), NullLogger<AnalyzeDemoHandler>.Instance);

		var result = await handler.Handle(new AnalyzeDemoCommand(Stream.Null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Contains("harnas", result.Value.TeamA.PlayerNames);
		Assert.Contains("Gracz …2001", result.Value.TeamB.PlayerNames);
		Assert.Contains("bravo", result.Value.TeamB.PlayerNames);
		Assert.Contains(result.Value.Players, p => p.DemoPlayerName == "harnas");
	}

	#endregion
}
