namespace HarnasHub.Application.Abstractions;

/// <summary>
/// Pushes a live-update signal to connected clients (SignalR) so the frontend can refetch the affected data
/// instead of polling. <paramref name="topic"/> matches a TanStack Query key prefix on the frontend
/// (e.g. "calendar", "tasks", "dashboard", "results", "availability:{eventId}").
/// </summary>
public interface IRealtimeNotifier
{
	Task NotifyAsync(string topic, CancellationToken cancellationToken = default);
}
