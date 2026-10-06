namespace HarnasHub.Application.Abstractions;

/// <summary>Reports whether optional configuration values (never the values themselves) are set, for the admin status page.</summary>
public interface IIntegrationSettings
{
	#region Public Properties

	/// <summary>True when a Discord webhook URL is configured.</summary>
	bool IsDiscordWebhookConfigured { get; }

	/// <summary>True when the public frontend base URL is configured.</summary>
	bool IsFrontendBaseUrlConfigured { get; }

	#endregion
}
