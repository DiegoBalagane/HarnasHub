#region Usings

using System.Threading.Channels;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Jobs;

#endregion

namespace HarnasHub.Infrastructure.Jobs;

/// <summary>In-process <see cref="IJobQueue"/>: one unbounded channel of job ids per <see cref="JobLane"/>. Only ids travel
/// through it (the job itself is in the database), and it lives only as long as the process — see
/// <c>JobStateStore.RecoverInterruptedAsync</c> for what happens to queued jobs after a restart.</summary>
public sealed class BackgroundJobQueue : IJobQueue
{
	#region Private Fields

	private readonly Dictionary<JobLane, Channel<Guid>> _channels = Enum.GetValues<JobLane>()
		.ToDictionary(lane => lane, _ => Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions { SingleReader = false, SingleWriter = false }));

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public void Enqueue(Guid jobId, JobLane lane)
	{
		if (!_channels[lane].Writer.TryWrite(jobId))
		{
			throw new InvalidOperationException($"Kolejka zadań {lane} jest zamknięta.");
		}
	}

	/// <summary>The reader the worker's consumers of <paramref name="lane"/> share.</summary>
	public ChannelReader<Guid> Reader(JobLane lane) => _channels[lane].Reader;

	#endregion
}
