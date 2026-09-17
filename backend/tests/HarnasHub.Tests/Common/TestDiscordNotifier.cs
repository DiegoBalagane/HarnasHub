using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IDiscordNotifier"/> recording the messages a handler sent.</summary>
public class TestDiscordNotifier : IDiscordNotifier
{
	#region Public Properties

	public List<string> Messages { get; } = [];

	#endregion

	#region Public Methods

	public Task SendAsync(string message, CancellationToken cancellationToken = default)
	{
		Messages.Add(message);
		return Task.CompletedTask;
	}

	#endregion
}
