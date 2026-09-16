namespace HarnasHub.Application.Abstractions;

/// <summary>Posts messages to the team's Discord channel via a configured webhook. A no-op when unconfigured.</summary>
public interface IDiscordNotifier
{
	Task SendAsync(string message, CancellationToken cancellationToken = default);
}
