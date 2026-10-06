using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using HarnasHub.Core.Entities;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.FaceitTestData;

namespace HarnasHub.Tests.Application.Features.OpponentReport.SyncOpponentFaceit;

public class SyncOpponentFaceitHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_refuse_when_faceit_is_not_configured()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFaceitClient(isConfigured: false)).Handle(new("Team X", true), CancellationToken.None);

		Assert.Equal("OpponentReport.FaceitNotConfigured", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_require_a_link()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await Handler(dbContext, new TestFaceitClient()).Handle(new("Team X", true), CancellationToken.None);

		Assert.Equal("OpponentReport.NotLinked", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_rate_limit_manual_refreshes_but_not_background_ones()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddLink(dbContext, lastSyncedAtUtc: DateTime.UtcNow.AddMinutes(-3));
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var handler = Handler(dbContext, new TestFaceitClient());

		var manual = await handler.Handle(new("Team X", IsManual: true), CancellationToken.None);
		var background = await handler.Handle(new("Team X", IsManual: false), CancellationToken.None);

		Assert.Equal("OpponentReport.RefreshTooSoon", manual.FirstError.Code);
		Assert.Contains("7 min", manual.FirstError.Description);
		Assert.False(background.IsError);
	}

	[Fact]
	public async Task Should_cache_team_games_once_and_store_the_report_snapshot()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddLink(dbContext, lastSyncedAtUtc: null);
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "Me", SteamId64 = "765" });
		dbContext.Events.Add(new Event { Id = Guid.NewGuid(), Title = "Mecz", Opponent = "team x", StartsAtUtc = DateTime.UtcNow.AddDays(2) });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient();
		client.PlayersBySteamId["765"] = new FaceitPlayerInfo("u1", "Me", "765", 2000, 9);
		var finishedAt = DateTime.UtcNow.AddDays(-1);
		foreach (var player in Them)
		{
			client.Histories[player] = [new FaceitHistoryItem("m1", finishedAt, "championship", "ESEA", "FINISHED")];
		}
		client.Stats["m1"] = [TestFaceitClient.MapStats("de_mirage", Them, Strangers())];

		var progress = new TestJobProgress();

		var result = await Handler(dbContext, client, progress).Handle(new("Team X", IsManual: true), CancellationToken.None);

		Assert.False(result.IsError);
		// The slow history step reports per-player progress instead of sitting on one percentage.
		Assert.True(progress.StepFractions.Count >= Them.Length);
		Assert.Equal(progress.StepFractions.Order(), progress.StepFractions);
		Assert.Equal(["m1"], client.StatsRequests);
		Assert.Contains("u1", client.HistoryRequests);
		Assert.Equal(1, result.Value.TheirTeamGames);
		Assert.True(result.Value.FaceitConfigured);
		Assert.NotNull(result.Value.NextEventId);
		Assert.Equal(10, await dbContext.FaceitMatchPlayerStats.CountAsync());
		Assert.NotNull((await dbContext.OpponentFaceitLinks.SingleAsync()).LastSyncedAtUtc);
		Assert.Single(dbContext.OpponentReportSnapshots);
	}

	[Fact]
	public async Task Should_return_a_failure_when_faceit_is_down()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		AddLink(dbContext, lastSyncedAtUtc: null);
		dbContext.Users.Add(new User { Id = Guid.NewGuid(), DiscordId = "1", DisplayName = "Me", SteamId64 = "765" });
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var client = new TestFaceitClient { ThrowOnCall = new HttpRequestException("429") };

		var result = await Handler(dbContext, client).Handle(new("Team X", IsManual: true), CancellationToken.None);

		Assert.Equal("OpponentReport.FaceitUnavailable", result.FirstError.Code);
		Assert.Null((await dbContext.OpponentFaceitLinks.SingleAsync()).LastSyncedAtUtc);
	}

	#endregion

	#region Private Methods

	private static SyncOpponentFaceitHandler Handler(
		TestApplicationDbContext dbContext, TestFaceitClient client, TestJobProgress? progress = null) =>
		new(dbContext, client, progress ?? new TestJobProgress(), NullLogger<SyncOpponentFaceitHandler>.Instance);

	private static void AddLink(TestApplicationDbContext dbContext, DateTime? lastSyncedAtUtc) =>
		dbContext.OpponentFaceitLinks.Add(new OpponentFaceitLink
		{
			Id = Guid.NewGuid(),
			OpponentKey = "team x",
			DisplayName = "Team X",
			PlayerIds = [.. Them],
			LinkedAtUtc = DateTime.UtcNow.AddDays(-1),
			LastSyncedAtUtc = lastSyncedAtUtc
		});

	#endregion
}
