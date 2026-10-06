using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IDiscordNotifier"/> recording the messages (and their channels) a handler sent.</summary>
public class TestDiscordNotifier : IDiscordNotifier
{
	#region Public Properties

	public List<string> Messages { get; } = [];

	public List<(DiscordChannel Channel, string Message)> Sent { get; } = [];

	#endregion

	#region Public Methods

	public Task SendAsync(DiscordChannel channel, string message, CancellationToken cancellationToken = default)
	{
		Messages.Add(message);
		Sent.Add((channel, message));
		return Task.CompletedTask;
	}

	#endregion
}
