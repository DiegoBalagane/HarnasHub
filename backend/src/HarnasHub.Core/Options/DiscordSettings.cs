#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Core.Options;

/// <summary>Configuration for posting team notifications to Discord: one webhook per channel plus a shared fallback.</summary>
public class DiscordSettings
{
	#region Public Fields

	public const string SectionName = "Discord";

	#endregion

	#region Public Properties

	/// <summary>Fallback incoming webhook URL used by every channel whose own URL is empty. Empty everywhere disables notifications.</summary>
	public string WebhookUrl { get; set; } = string.Empty;

	/// <summary>Per-channel webhook URLs (<c>Discord:Webhooks:{Channel}</c>); an empty one falls back to <see cref="WebhookUrl"/>.</summary>
	public DiscordWebhooksSettings Webhooks { get; set; } = new();

	#endregion

	#region Public Methods

	/// <summary>The webhook URL for <paramref name="channel"/> (its own, else the fallback); empty when neither is configured.</summary>
	public string GetWebhookUrl(DiscordChannel channel)
	{
		var own = channel switch
		{
			DiscordChannel.Announcements => Webhooks.Announcements,
			DiscordChannel.MatchSchedule => Webhooks.MatchSchedule,
			DiscordChannel.DemoReview => Webhooks.DemoReview,
			DiscordChannel.OpponentScouting => Webhooks.OpponentScouting,
			_ => string.Empty
		};

		return (string.IsNullOrWhiteSpace(own) ? WebhookUrl : own)?.Trim() ?? string.Empty;
	}

	/// <summary>True when <paramref name="channel"/> has a usable webhook, directly or through the fallback.</summary>
	public bool IsConfigured(DiscordChannel channel) => GetWebhookUrl(channel).Length > 0;

	#endregion
}

/// <summary>Webhook URLs of the four notification channels.</summary>
public class DiscordWebhooksSettings
{
	/// <summary>Webhook of the announcements channel (new events, tasks, regular reminders).</summary>
	public string Announcements { get; set; } = string.Empty;

	/// <summary>Webhook of the match schedule channel (match events, match reminders, results).</summary>
	public string MatchSchedule { get; set; } = string.Empty;

	/// <summary>Webhook of the demo review channel (digest after a demo is attached to a result).</summary>
	public string DemoReview { get; set; } = string.Empty;

	/// <summary>Webhook of the opponent scouting channel (briefing, opponent demo digests).</summary>
	public string OpponentScouting { get; set; } = string.Empty;
}
