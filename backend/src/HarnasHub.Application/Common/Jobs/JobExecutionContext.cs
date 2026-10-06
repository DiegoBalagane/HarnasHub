namespace HarnasHub.Application.Common.Jobs;

/// <summary>Scoped marker of "this DI scope executes background job X for user Y" — set by the worker before dispatching
/// the job's request, read by <see cref="JobProgress"/> and by the API's current-user service (no HTTP context in a job).</summary>
public sealed class JobExecutionContext
{
	#region Public Properties

	/// <summary>The running job, null outside a job.</summary>
	public Guid? JobId { get; private set; }

	/// <summary>Who started the job, null outside a job.</summary>
	public Guid? RequestedByUserId { get; private set; }

	/// <summary>Where progress goes, null outside a job.</summary>
	public IJobProgressSink? Sink { get; private set; }

	#endregion

	#region Public Methods

	/// <summary>Marks the scope as executing <paramref name="jobId"/>.</summary>
	public void Begin(Guid jobId, Guid requestedByUserId, IJobProgressSink sink)
	{
		JobId = jobId;
		RequestedByUserId = requestedByUserId;
		Sink = sink;
	}

	#endregion
}
