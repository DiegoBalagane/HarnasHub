using System.Net;
using System.Text.Json;
using HarnasHub.Application.Common.Notifications;
using HarnasHub.Core.Enums;
using HarnasHub.Core.Options;
using HarnasHub.Infrastructure.Notifications;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace HarnasHub.Tests.Infrastructure.Notifications;

public class DiscordWebhookNotifierTests
{
	#region Public Methods

	[Fact]
	public async Task Does_nothing_when_the_channel_has_no_webhook()
	{
		var handler = new RecordingHandler();

		await Notifier(handler, new DiscordSettings()).SendAsync(DiscordChannel.Announcements, "hej");

		Assert.Empty(handler.Requests);
	}

	[Fact]
	public async Task Posts_to_the_channel_own_webhook()
	{
		var handler = new RecordingHandler();
		var settings = new DiscordSettings
		{
			WebhookUrl = "https://discord.test/fallback",
			Webhooks = new DiscordWebhooksSettings { MatchSchedule = "https://discord.test/match" }
		};

		await Notifier(handler, settings).SendAsync(DiscordChannel.MatchSchedule, "mecz");

		var request = Assert.Single(handler.Requests);
		Assert.Equal("https://discord.test/match", request.Url);
		Assert.Equal("mecz", JsonDocument.Parse(request.Body).RootElement.GetProperty("content").GetString());
	}

	[Fact]
	public async Task Falls_back_to_the_shared_webhook()
	{
		var handler = new RecordingHandler();

		await Notifier(handler, new DiscordSettings { WebhookUrl = "https://discord.test/fallback" }).SendAsync(DiscordChannel.DemoReview, "demo");

		Assert.Equal("https://discord.test/fallback", Assert.Single(handler.Requests).Url);
	}

	[Fact]
	public async Task Truncates_messages_above_the_discord_limit()
	{
		var handler = new RecordingHandler();

		await Notifier(handler, new DiscordSettings { WebhookUrl = "https://discord.test/f" })
			.SendAsync(DiscordChannel.Announcements, new string('x', 5000));

		var content = JsonDocument.Parse(Assert.Single(handler.Requests).Body).RootElement.GetProperty("content").GetString()!;
		Assert.True(content.Length <= DiscordMessage.MaxLength);
	}

	[Fact]
	public async Task Failures_are_swallowed()
	{
		var settings = new DiscordSettings { WebhookUrl = "https://discord.test/f" };

		await Notifier(new RecordingHandler(HttpStatusCode.InternalServerError), settings).SendAsync(DiscordChannel.Announcements, "a");
		await Notifier(new RecordingHandler(throwError: true), settings).SendAsync(DiscordChannel.Announcements, "a");
	}

	[Fact]
	public void Frontend_links_are_omitted_without_a_base_url()
	{
		Assert.Null(FrontendLinks.Build("  ", "/results/1"));
		Assert.Equal("https://hub.test/results/1", FrontendLinks.Build("https://hub.test/", "/results/1"));
	}

	#endregion

	#region Private Methods

	private static DiscordWebhookNotifier Notifier(RecordingHandler handler, DiscordSettings settings) =>
		new(new HttpClient(handler), Options.Create(settings), NullLogger<DiscordWebhookNotifier>.Instance);

	#endregion

	private sealed class RecordingHandler(HttpStatusCode status = HttpStatusCode.NoContent, bool throwError = false) : HttpMessageHandler
	{
		public List<(string Url, string Body)> Requests { get; } = [];

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (throwError)
			{
				throw new HttpRequestException("boom");
			}

			Requests.Add((request.RequestUri!.ToString(), await request.Content!.ReadAsStringAsync(cancellationToken)));
			return new HttpResponseMessage(status);
		}
	}
}
