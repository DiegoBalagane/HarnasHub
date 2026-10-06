namespace HarnasHub.Application.Common.Jobs;

/// <summary>Worker pool a job kind runs on — each lane has its own concurrency limit.</summary>
public enum JobLane
{
	/// <summary>Demo parsing (memory-heavy, a few at a time).</summary>
	Demo = 0,

	/// <summary>FACEIT Data API calls (network-bound).</summary>
	Faceit = 1
}
