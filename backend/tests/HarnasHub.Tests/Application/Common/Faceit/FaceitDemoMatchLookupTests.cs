#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Faceit;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Faceit;

public class FaceitDemoMatchLookupTests
{
	#region Public Fields

	/// <summary>FACEIT match id used by the demo-name fixtures.</summary>
	public const string MatchId = "1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e";

	/// <summary>A FACEIT demo file name pointing at <see cref="MatchId"/>, map 1.</summary>
	public const string FileName = MatchId + "-1-1.dem";

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_find_the_match_our_faction_and_an_already_linked_opponent()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "Me", SteamId64 = "101" });
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink
		{
			Id = Guid.NewGuid(),
			OpponentKey = "rivals",
			DisplayName = "Rivals",
			PlayerIds = ["b1", "b2"]
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = ClientWithMatch();

		var result = await TestFaceitLookup.Create(dbContext, client).FindAsync(FileName, CancellationToken.None);

		Assert.NotNull(result.Match);
		Assert.Null(result.Note);
		Assert.Equal(0, result.Match.OurFactionIndex);
		Assert.Equal([null, "Rivals"], result.Match.LinkedOpponentNames);
		Assert.Equal(1, result.Match.Reference.MapNumber);
	}

	[Fact]
	public async Task Should_say_nothing_for_a_name_that_is_not_a_faceit_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await TestFaceitLookup.Create(dbContext, ClientWithMatch()).FindAsync("scrim.dem", CancellationToken.None);

		Assert.Equal(FaceitDemoLookupResult.None, result);
	}

	[Fact]
	public async Task Should_only_leave_a_note_without_api_key_or_when_faceit_fails()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var failing = ClientWithMatch();
		failing.ThrowOnCall = new HttpRequestException("down");

		var notConfigured = await TestFaceitLookup.Create(dbContext).FindAsync(FileName, CancellationToken.None);
		var unavailable = await TestFaceitLookup.Create(dbContext, failing).FindAsync(FileName, CancellationToken.None);
		var unknown = await TestFaceitLookup.Create(dbContext, new TestFaceitClient()).FindAsync(FileName, CancellationToken.None);

		Assert.Equal((null, FaceitDemoMatchLookup.NotConfiguredNote), (notConfigured.Match, notConfigured.Note));
		Assert.Equal((null, FaceitDemoMatchLookup.UnavailableNote), (unavailable.Match, unavailable.Note));
		Assert.Equal((null, FaceitDemoMatchLookup.UnavailableNote), (unknown.Match, unknown.Note));
	}

	/// <summary>A configured FACEIT client knowing <see cref="MatchId"/>: faction 1 holds SteamID 101/102, faction 2 201/202.</summary>
	public static TestFaceitClient ClientWithMatch()
	{
		var client = new TestFaceitClient();
		client.Matches[MatchId] = new FaceitMatchInfo(MatchId,
		[
			new FaceitFactionInfo("f1", "team_Alpha", [new("a1", "Alpha", "101", 10), new("a2", "Bravo", "102", 9)]),
			new FaceitFactionInfo("f2", "team_Charlie", [new("b1", "Charlie", "201", 8), new("b2", "Delta", "202", 8)])
		])
		{
			CompetitionType = "championship",
			CompetitionName = "Weekend Cup",
			StartedAtUtc = new DateTime(2026, 10, 1, 18, 0, 0, DateTimeKind.Utc),
			PickedMaps = ["de_mirage"]
		};
		return client;
	}

	#endregion
}
