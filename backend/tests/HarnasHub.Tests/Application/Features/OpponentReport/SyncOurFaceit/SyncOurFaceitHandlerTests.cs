using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.SyncOurFaceit;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.SyncOurFaceit;

public class SyncOurFaceitHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_refuse_when_faceit_is_not_configured()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFaceitClient(isConfigured: false)).Handle(new SyncOurFaceitCommand(), CancellationToken.None);

		Assert.Equal("OpponentReport.FaceitNotConfigured", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_resolve_players_by_steam_id_and_skip_matches_already_cached()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.AddRange(
			new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "A", SteamId64 = " 111 " },
			new User { Id = Guid.NewGuid(), DiscordId = "2", DisplayName = "B", SteamId64 = "222" },
			new User { Id = Guid.NewGuid(), DiscordId = "3", DisplayName = "No steam" });
		dbContext.FaceitMatches.Add(Match("de_nuke", Us, Strangers(), 13, 2, DateTime.UtcNow.AddDays(-5), matchId: "old"));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();
		client.PlayersBySteamId["111"] = new FaceitPlayerInfo("u1", "A", "111", 1500, 6);
		var playedAt = DateTime.UtcNow.AddDays(-1);
		client.Histories["u1"] =
		[
			new FaceitHistoryItem("old", playedAt, null, null, "FINISHED"),
			new FaceitHistoryItem("new", playedAt, null, null, "FINISHED"),
			new FaceitHistoryItem("cancelled", playedAt, null, null, "CANCELLED")
		];
		client.Stats["new"] = [TestFaceitClient.MapStats("de_ancient", Us, Strangers())];

		var result = await Handler(dbContext, client).Handle(new SyncOurFaceitCommand(), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(new(1, 1, true), result.Value);
		Assert.Equal(["new"], client.StatsRequests);
		Assert.Equal(2, await dbContext.FaceitMatches.CountAsync());
		Assert.NotNull((await dbContext.FaceitPlayers.FindAsync("u1"))!.HistorySyncedAtUtc);
	}

	[Fact]
	public async Task Should_not_pull_history_again_within_the_resync_interval()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "A", SteamId64 = "111" });
		dbContext.FaceitPlayers.Add(new FaceitPlayer
		{
			Id = "u1",
			Nickname = "A",
			SteamId64 = "111",
			UpdatedAtUtc = DateTime.UtcNow,
			HistorySyncedAtUtc = DateTime.UtcNow.AddMinutes(-2)
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();

		var result = await Handler(dbContext, client).Handle(new SyncOurFaceitCommand(), CancellationToken.None);

		Assert.Equal(1, result.Value.Players);
		Assert.Empty(client.HistoryRequests);
	}

	#endregion

	#region Private Methods

	private static SyncOurFaceitHandler Handler(TestApplicationDbContext dbContext, TestFaceitClient client) =>
		new(dbContext, client, NullLogger<SyncOurFaceitHandler>.Instance);

	#endregion
}
