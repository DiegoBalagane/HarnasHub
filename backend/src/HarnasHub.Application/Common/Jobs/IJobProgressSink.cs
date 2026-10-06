namespace HarnasHub.Application.Common.Jobs;

/// <summary>Receives absolute progress updates of one running job (Infrastructure persists and pushes them, throttled).
/// Must be thread-safe and non-blocking: it is called from inside stream reads.</summary>
public interface IJobProgressSink
{
	/// <summary>Publishes <paramref name="percent"/> (0–100) with the current stage label.</summary>
	void Publish(int percent, string? stage);
}
