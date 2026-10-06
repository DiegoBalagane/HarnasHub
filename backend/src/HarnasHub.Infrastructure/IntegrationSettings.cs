using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure;

/// <summary>Reads presence of optional settings from bound options, exposing booleans only.</summary>
public class IntegrationSettings(IOptions<DiscordSettings> discord, IOptions<FrontendSettings> frontend) : IIntegrationSettings
{
	#region Public Properties

	/// <inheritdoc />
	public bool IsDiscordWebhookConfigured => Enum.GetValues<DiscordChannel>().Any(IsDiscordChannelConfigured);

	/// <inheritdoc />
	public bool IsFrontendBaseUrlConfigured => !string.IsNullOrWhiteSpace(frontend.Value.BaseUrl);

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public bool IsDiscordChannelConfigured(DiscordChannel channel) => discord.Value.IsConfigured(channel);

	#endregion
}
