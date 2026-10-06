using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Common.Notifications;

public class TeamNotificationsTests
{
	#region Public Methods

	[Fact]
	public async Task Result_saved_goes_to_the_match_schedule_with_a_link()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var discord = new TestDiscordNotifier();
		var result = Result();

		await TestTeamNotifications.Create(dbContext, discord: discord).NotifyResultSavedAsync(result, CancellationToken.None);

		var (channel, message) = Assert.Single(discord.Sent);
		Assert.Equal(DiscordChannel.MatchSchedule, channel);
		Assert.Contains("**13:7**", message);
		Assert.EndsWith($"https://hub.test/results/{result.Id}", message);
	}

	[Fact]
	public async Task Result_saved_has_no_link_without_a_frontend_base_url()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var discord = new TestDiscordNotifier();

		await TestTeamNotifications.Create(dbContext, discord: discord, baseUrl: null).NotifyResultSavedAsync(Result(), CancellationToken.None);

		Assert.DoesNotContain("🔗", Assert.Single(discord.Sent).Message);
	}

	[Fact]
	public async Task Demo_review_without_a_stored_timeline_posts_nothing_and_does_not_throw()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var result = Result();
		dbContext.MatchResults.Add(result);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var discord = new TestDiscordNotifier();

		await TestTeamNotifications.Create(dbContext, discord: discord).NotifyDemoReviewAsync(result.Id, CancellationToken.None);

		Assert.Empty(discord.Sent);
	}

	[Fact]
	public async Task Opponent_digest_goes_to_the_scouting_channel_with_a_report_link()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var discord = new TestDiscordNotifier();

		await TestTeamNotifications.Create(dbContext, discord: discord).NotifyOpponentDemosAsync("team x", "Team X", ["Mirage"], CancellationToken.None);

		var (channel, message) = Assert.Single(discord.Sent);
		Assert.Equal(DiscordChannel.OpponentScouting, channel);
		Assert.Contains("Team X", message);
		Assert.EndsWith("https://hub.test/opponents/report?name=Team%20X", message);
	}

	#endregion

	#region Private Methods

	private static MatchResult Result() => new()
	{
		Id = Guid.NewGuid(),
		Opponent = "Team X",
		OurScore = 13,
		OpponentScore = 7,
		MapName = "Mirage",
		PlayedAtUtc = DateTime.UtcNow,
		CreatedAtUtc = DateTime.UtcNow
	};

	#endregion
}
