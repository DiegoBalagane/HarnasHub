using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.LinkOpponentFaceit;

public class LinkOpponentFaceitHandlerTests
{
	#region Private Fields

	private const string TeamId = "0b1c2d3e-1111-2222-3333-444455556666";
	private const string MatchId = "1-0b1c2d3e-1111-2222-3333-444455556666";
	private readonly Guid _userId = Guid.NewGuid();

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_refuse_when_faceit_is_not_configured()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFaceitClient(isConfigured: false)).Handle(Command("alpha"), CancellationToken.None);

		Assert.Equal("OpponentReport.FaceitNotConfigured", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_link_nicknames_cache_players_and_drop_the_old_snapshot()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.OpponentReportSnapshots.Add(new OpponentReportSnapshot { Id = Guid.NewGuid(), OpponentKey = "team x", ReportJson = "{}" });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();
		client.PlayersByNickname["alpha"] = new FaceitPlayerInfo("p-a", "Alpha", "7656", 2100, 10);
		client.PlayersByNickname["beta"] = new FaceitPlayerInfo("p-b", "Beta", null, 1800, 8);

		var result = await Handler(dbContext, client).Handle(Command("alpha, beta"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["p-a", "p-b"], result.Value.Players.Select(p => p.PlayerId));
		var link = await dbContext.OpponentFaceitLinks.SingleAsync();
		Assert.Equal(("team x", "Team X", _userId), (link.OpponentKey, link.DisplayName, link.LinkedByUserId));
		Assert.Equal(2100, (await dbContext.FaceitPlayers.FindAsync("p-a"))!.Elo);
		Assert.Empty(dbContext.OpponentReportSnapshots);
	}

	[Fact]
	public async Task Should_report_every_unknown_nickname()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var client = new TestFaceitClient();
		client.PlayersByNickname["alpha"] = new FaceitPlayerInfo("p-a", "Alpha", null, null, null);

		var result = await Handler(dbContext, client).Handle(Command("alpha ghost1 ghost2"), CancellationToken.None);

		Assert.Equal("OpponentReport.PlayersNotFound", result.FirstError.Code);
		Assert.Contains("ghost1, ghost2", result.FirstError.Description);
		Assert.Empty(dbContext.OpponentFaceitLinks);
	}

	[Fact]
	public async Task Should_link_every_member_of_a_team_and_keep_the_team_id()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var client = new TestFaceitClient();
		client.Teams[TeamId] = new FaceitTeamInfo(TeamId, "Team X", [Ref("m1"), Ref("m2"), Ref("m3")]);

		var result = await Handler(dbContext, client).Handle(Command($"https://www.faceit.com/en/teams/{TeamId}"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(TeamId, result.Value.FaceitTeamId);
		Assert.Equal(3, (await dbContext.OpponentFaceitLinks.SingleAsync()).PlayerIds.Count);
	}

	[Fact]
	public async Task Should_take_the_faction_without_our_players_from_a_match_room()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "Me", SteamId64 = "76561198000000001" });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();
		client.Matches[MatchId] = new FaceitMatchInfo(MatchId,
		[
			new FaceitFactionInfo("f1", "team_Me", [Ref("our1", "76561198000000001"), Ref("our2")]),
			new FaceitFactionInfo("f2", "team_Them", [Ref("their1"), Ref("their2")])
		]);

		var result = await Handler(dbContext, client).Handle(Command($"https://www.faceit.com/en/cs2/room/{MatchId}"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(["their1", "their2"], result.Value.Players.Select(p => p.PlayerId));
	}

	[Fact]
	public async Task Should_refuse_a_match_room_it_cannot_attribute()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var client = new TestFaceitClient();
		client.Matches[MatchId] = new FaceitMatchInfo(MatchId,
		[
			new FaceitFactionInfo("f1", "team_A", [Ref("a1")]),
			new FaceitFactionInfo("f2", "team_B", [Ref("b1")])
		]);

		var result = await Handler(dbContext, client).Handle(Command($"https://www.faceit.com/en/cs2/room/{MatchId}"), CancellationToken.None);

		Assert.Equal("OpponentReport.AmbiguousMatch", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_return_a_failure_when_faceit_is_down()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var client = new TestFaceitClient { ThrowOnCall = new HttpRequestException("503") };

		var result = await Handler(dbContext, client).Handle(Command("alpha"), CancellationToken.None);

		Assert.Equal("OpponentReport.FaceitUnavailable", result.FirstError.Code);
	}

	#endregion

	#region Private Methods

	private LinkOpponentFaceitHandler Handler(TestApplicationDbContext dbContext, TestFaceitClient client) =>
		new(dbContext, client, new TestCurrentUserService(_userId, "Coach", isCoach: true), NullLogger<LinkOpponentFaceitHandler>.Instance);

	private static LinkOpponentFaceitCommand Command(string source) => new(" Team X ", source);

	private static FaceitPlayerRef Ref(string id, string? steamId = null) => new(id, $"nick-{id}", steamId, 5);

	#endregion
}
