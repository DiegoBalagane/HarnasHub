#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Common.Jobs;

/// <summary>Realtime topics of background jobs: <c>job:{id}</c> for one job and <c>jobs:{userId}</c> for everything a user
/// started. The push carries only the topic — clients refetch <c>GET /api/jobs/{id}</c>, which enforces access.</summary>
public static class JobTopics
{
	#region Public Methods

	/// <summary>Topic of one job.</summary>
	public static string Job(Guid jobId) => $"job:{jobId}";

	/// <summary>Topic of every job a user started.</summary>
	public static string User(Guid userId) => $"jobs:{userId}";

	/// <summary>Pushes both topics of a job; a failed push is never allowed to fail the job (clients also poll).</summary>
	public static async Task NotifyAsync(IRealtimeNotifier notifier, Guid jobId, Guid userId)
	{
		try
		{
			await notifier.NotifyAsync(Job(jobId), CancellationToken.None);
			await notifier.NotifyAsync(User(userId), CancellationToken.None);
		}
		catch (Exception)
		{
			// Best-effort: the frontend falls back to polling GET /api/jobs/{id}.
		}
	}

	#endregion
}
