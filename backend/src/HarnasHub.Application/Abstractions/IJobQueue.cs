#region Usings

using HarnasHub.Application.Common.Jobs;

#endregion

namespace HarnasHub.Application.Abstractions;

/// <summary>Hands a persisted, queued job to the background worker. In-process (Infrastructure, System.Threading.Channels):
/// the queue does not survive a restart — jobs left queued or running are marked failed on the next start.</summary>
public interface IJobQueue
{
	/// <summary>Queues the job on its lane; the worker picks it up as soon as a slot of that lane is free.</summary>
	void Enqueue(Guid jobId, JobLane lane);
}
