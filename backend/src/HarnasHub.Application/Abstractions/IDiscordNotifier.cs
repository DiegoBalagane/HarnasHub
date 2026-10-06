using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Abstractions;

/// <summary>Posts messages to one of the team's Discord channels via its webhook. A no-op when the channel is unconfigured; never throws.</summary>
public interface IDiscordNotifier
{
	/// <summary>Sends <paramref name="message"/> (truncated to Discord's limit) to <paramref name="channel"/>.</summary>
	Task SendAsync(DiscordChannel channel, string message, CancellationToken cancellationToken = default);
}
