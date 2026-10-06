using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Abstractions;

/// <summary>Reports whether optional configuration values (never the values themselves) are set, for the admin status page.</summary>
public interface IIntegrationSettings
{
	#region Public Properties

	/// <summary>True when at least one Discord channel has a webhook (its own or the fallback).</summary>
	bool IsDiscordWebhookConfigured { get; }

	/// <summary>True when the public frontend base URL is configured.</summary>
	bool IsFrontendBaseUrlConfigured { get; }

	#endregion

	#region Public Methods

	/// <summary>True when <paramref name="channel"/> has a webhook, directly or through the <c>Discord:WebhookUrl</c> fallback.</summary>
	bool IsDiscordChannelConfigured(DiscordChannel channel);

	#endregion
}
