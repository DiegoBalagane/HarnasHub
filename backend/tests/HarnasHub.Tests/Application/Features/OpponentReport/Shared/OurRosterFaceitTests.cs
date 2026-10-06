using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class OurRosterFaceitTests
{
	#region Private Fields

	private static readonly DateTime Now = new(2026, 10, 6, 12, 0, 0, DateTimeKind.Utc);

	#endregion

	#region Public Methods

	[Fact]
	public async Task Should_resolve_by_steam_id_first_and_not_ask_for_the_nickname()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(NewUser("A", steamId: "111", faceitNickname: "other"));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();
		client.PlayersBySteamId["111"] = new FaceitPlayerInfo("u1", "A", "111", 1500, 6);
		client.PlayersByNickname["other"] = new FaceitPlayerInfo("u2", "other", null, 1000, 3);

		var ids = await OurRosterFaceit.ResolveAsync(dbContext, client, Now, CancellationToken.None);

		Assert.Equal(["u1"], ids);
	}

	[Fact]
	public async Task Should_fall_back_to_the_nickname_when_the_steam_id_has_no_faceit_account()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(NewUser("A", steamId: "111", faceitNickname: "Nick"));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();
		client.PlayersByNickname["Nick"] = new FaceitPlayerInfo("u9", "Nick", null, 1200, 4);

		var ids = await OurRosterFaceit.ResolveAsync(dbContext, client, Now, CancellationToken.None);

		Assert.Equal(["u9"], ids);
		Assert.Equal("Nick", (await dbContext.FaceitPlayers.FindAsync("u9"))!.Nickname);
	}

	[Fact]
	public async Task Should_resolve_a_user_without_steam_id_by_nickname_and_reuse_the_fresh_cache()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(NewUser("A", faceitNickname: "nick"));
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "u9", Nickname = "NICK", UpdatedAtUtc = Now.AddHours(-1) });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient { ThrowOnCall = new HttpRequestException("must not be called") };

		var ids = await OurRosterFaceit.ResolveAsync(dbContext, client, Now, CancellationToken.None);

		Assert.Equal(["u9"], ids);
	}

	[Fact]
	public async Task Should_skip_users_hidden_from_stats()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(NewUser("A", steamId: "111", showInStats: false));
		dbContext.Users.Add(NewUser("B", faceitNickname: "nick", showInStats: false));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient { ThrowOnCall = new HttpRequestException("must not be called") };

		var ids = await OurRosterFaceit.ResolveAsync(dbContext, client, Now, CancellationToken.None);

		Assert.Empty(ids);
	}

	[Fact]
	public async Task Should_report_each_unresolved_roster_player_with_the_matching_reason()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.AddRange(
			NewUser("Nothing", slot: RosterSlot.Main),
			NewUser("OnlySteam", steamId: "222", slot: RosterSlot.Main),
			NewUser("BadNick", faceitNickname: "ghost", slot: RosterSlot.Bench),
			NewUser("Resolved", steamId: "111", slot: RosterSlot.Main),
			NewUser("Hidden", slot: RosterSlot.Main, showInStats: false),
			NewUser("StandIn", slot: RosterSlot.StandIn),
			NewUser("Guest", slot: RosterSlot.Main, accessLevel: AccessLevel.Guest));
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "u1", Nickname = "R", SteamId64 = "111", UpdatedAtUtc = Now });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var (players, unresolved) = await OurRosterFaceit.LoadCachedAsync(dbContext, CancellationToken.None);

		Assert.Equal(["u1"], players.Select(p => p.Id));
		Assert.Equal(
			[
				("BadNick", OurRosterFaceit.NicknameNotFoundReason),
				("Nothing", OurRosterFaceit.NoIdentifiersReason),
				("OnlySteam", OurRosterFaceit.SteamNotLinkedReason)
			],
			unresolved.OrderBy(u => u.DisplayName).Select(u => (u.DisplayName, u.Reason)));
	}

	[Fact]
	public async Task Should_not_report_a_player_resolved_through_the_cached_nickname()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.Users.Add(NewUser("A", steamId: "222", faceitNickname: "Nick", slot: RosterSlot.Main));
		dbContext.FaceitPlayers.Add(new FaceitPlayer { Id = "u9", Nickname = "nick", UpdatedAtUtc = Now });
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var (players, unresolved) = await OurRosterFaceit.LoadCachedAsync(dbContext, CancellationToken.None);

		Assert.Equal(["u9"], players.Select(p => p.Id));
		Assert.Empty(unresolved);
	}

	#endregion

	#region Private Methods

	private static User NewUser(
		string name,
		string? steamId = null,
		string? faceitNickname = null,
		RosterSlot? slot = null,
		bool showInStats = true,
		AccessLevel accessLevel = AccessLevel.Player) => new()
		{
			Id = Guid.NewGuid(),
			DiscordId = Guid.NewGuid().ToString("N"),
			DisplayName = name,
			AccessLevel = accessLevel,
			RosterSlot = slot,
			SteamId64 = steamId,
			FaceitNickname = faceitNickname,
			ShowInStats = showInStats,
			CreatedAtUtc = Now
		};

	#endregion
}
