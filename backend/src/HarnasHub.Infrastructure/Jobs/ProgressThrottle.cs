namespace HarnasHub.Infrastructure.Jobs;

/// <summary>Decides which progress updates are worth a database write and a realtime push: every stage change, otherwise
/// only a higher percentage at least <paramref name="minInterval"/> after the last published one. Not thread-safe — the
/// caller serialises access.</summary>
public sealed class ProgressThrottle(TimeSpan minInterval)
{
	#region Private Fields

	private int _lastPercent = -1;
	private string? _lastStage;
	private DateTimeOffset _lastPublishedAt = DateTimeOffset.MinValue;

	#endregion

	#region Public Methods

	/// <summary>Whether to publish (<paramref name="percent"/>, <paramref name="stage"/>) at <paramref name="now"/>; remembers it when so.</summary>
	public bool ShouldPublish(int percent, string? stage, DateTimeOffset now)
	{
		var stageChanged = !string.Equals(stage, _lastStage, StringComparison.Ordinal);
		if (!stageChanged && (percent <= _lastPercent || now - _lastPublishedAt < minInterval))
		{
			return false;
		}

		_lastPercent = percent;
		_lastStage = stage;
		_lastPublishedAt = now;
		return true;
	}

	#endregion
}
