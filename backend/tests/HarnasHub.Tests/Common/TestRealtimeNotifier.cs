using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IRealtimeNotifier"/> recording the topics a handler pushed.</summary>
public class TestRealtimeNotifier : IRealtimeNotifier
{
	#region Public Properties

	public List<string> Topics { get; } = [];

	#endregion

	#region Public Methods

	public Task NotifyAsync(string topic, CancellationToken cancellationToken = default)
	{
		Topics.Add(topic);
		return Task.CompletedTask;
	}

	#endregion
}
