using HarnasHub.Core.Enums;
using HarnasHub.Core.Options;
using Xunit;

namespace HarnasHub.Tests.Settings;

public class DiscordSettingsTests
{
	#region Public Methods

	[Fact]
	public void Nothing_configured_means_no_channel_is_configured()
	{
		var settings = new DiscordSettings();

		Assert.All(Enum.GetValues<DiscordChannel>(), c =>
		{
			Assert.Equal(string.Empty, settings.GetWebhookUrl(c));
			Assert.False(settings.IsConfigured(c));
		});
	}

	[Fact]
	public void Each_channel_uses_its_own_webhook()
	{
		var settings = new DiscordSettings
		{
			Webhooks = new DiscordWebhooksSettings { Announcements = "a", MatchSchedule = "m", DemoReview = "d", OpponentScouting = "o" },
			WebhookUrl = "fallback"
		};

		Assert.Equal("a", settings.GetWebhookUrl(DiscordChannel.Announcements));
		Assert.Equal("m", settings.GetWebhookUrl(DiscordChannel.MatchSchedule));
		Assert.Equal("d", settings.GetWebhookUrl(DiscordChannel.DemoReview));
		Assert.Equal("o", settings.GetWebhookUrl(DiscordChannel.OpponentScouting));
	}

	[Fact]
	public void Empty_or_blank_channel_url_falls_back_to_the_shared_webhook()
	{
		var settings = new DiscordSettings
		{
			WebhookUrl = " fallback ",
			Webhooks = new DiscordWebhooksSettings { MatchSchedule = "m", DemoReview = "   " }
		};

		Assert.Equal("m", settings.GetWebhookUrl(DiscordChannel.MatchSchedule));
		Assert.Equal("fallback", settings.GetWebhookUrl(DiscordChannel.DemoReview));
		Assert.Equal("fallback", settings.GetWebhookUrl(DiscordChannel.Announcements));
		Assert.True(settings.IsConfigured(DiscordChannel.OpponentScouting));
	}

	[Fact]
	public void A_single_channel_url_without_fallback_configures_only_that_channel()
	{
		var settings = new DiscordSettings { Webhooks = new DiscordWebhooksSettings { DemoReview = "d" } };

		Assert.True(settings.IsConfigured(DiscordChannel.DemoReview));
		Assert.False(settings.IsConfigured(DiscordChannel.Announcements));
	}

	#endregion
}
