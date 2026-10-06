using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure;

/// <summary>Reads presence of optional settings from bound options, exposing booleans only.</summary>
public class IntegrationSettings(IOptions<DiscordSettings> discord, IOptions<FrontendSettings> frontend) : IIntegrationSettings
{
	#region Public Properties

	/// <inheritdoc />
	public bool IsDiscordWebhookConfigured => !string.IsNullOrWhiteSpace(discord.Value.WebhookUrl);

	/// <inheritdoc />
	public bool IsFrontendBaseUrlConfigured => !string.IsNullOrWhiteSpace(frontend.Value.BaseUrl);

	#endregion
}
