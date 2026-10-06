#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Faceit;
using HarnasHub.Application.Features.Results.AnalyzeDemo;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Common.Faceit;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.Results.AnalyzeDemo;

public class AnalyzeDemoFaceitPrefillTests
{
	#region Public Methods

	[Fact]
	public async Task Should_prefill_from_the_faceit_match_named_by_the_file()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "Me", SteamId64 = "101", CreatedAtUtc = DateTime.UtcNow });
		await dbContext.SaveChangesAsync();

		var result = await Handler(dbContext, FaceitDemoMatchLookupTests.ClientWithMatch())
			.Handle(new AnalyzeDemoCommand(Stream.Null, FaceitDemoMatchLookupTests.FileName), CancellationToken.None);

		Assert.False(result.IsError);
		var prefill = result.Value.FaceitMatch;
		Assert.NotNull(prefill);
		Assert.Null(result.Value.FaceitNote);
		Assert.Equal(FaceitDemoMatchLookupTests.MatchId, prefill.MatchId);
		Assert.Equal((MatchCategory.Tournament, "Mirage", (int?)0), (prefill.Category, prefill.MapName, prefill.OurFactionIndex));
		Assert.Equal(new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc), prefill.PlayedAtUtc);
		Assert.Equal(["B", "A"], prefill.Factions.Select(f => f.DemoTeam));
		Assert.Equal(["Charlie", "Delta"], prefill.Factions[1].Nicknames);
		Assert.Equal("B", result.Value.SuggestedTeam);
	}

	[Fact]
	public async Task Should_analyse_as_before_with_only_a_note_when_faceit_is_not_configured()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFaceitClient(isConfigured: false))
			.Handle(new AnalyzeDemoCommand(Stream.Null, FaceitDemoMatchLookupTests.FileName), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(result.Value.FaceitMatch);
		Assert.Equal(FaceitDemoMatchLookup.NotConfiguredNote, result.Value.FaceitNote);
		Assert.Equal((1, 0), (result.Value.TeamA.OurScore, result.Value.TeamA.OpponentScore));
	}

	#endregion

	#region Private Methods

	private static AnalyzeDemoHandler Handler(TestApplicationDbContext dbContext, TestFaceitClient client)
	{
		var stats = new Dictionary<int, int> { [2] = 0, [3] = 0, [4] = 0, [5] = 0 };
		DemoPlayerStats Player(long id, string name) => new(id, name, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, stats, []);
		var parsed = new DemoParseResult(1, MapName.Mirage,
			[Player(101, "Alpha"), Player(102, "Bravo"), Player(201, "Charlie"), Player(202, "Delta")],
			[new DemoRoundResult(MapSide.T, [201, 202], [101, 102])]);

		return new AnalyzeDemoHandler(
			new TestDemoParser(parsed), dbContext, new TestFileStorage(isConfigured: false),
			TestFaceitLookup.Create(dbContext, client), NullLogger<AnalyzeDemoHandler>.Instance);
	}

	#endregion
}
