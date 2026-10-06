#region Usings

using HarnasHub.Application.Common.Jobs;
using HarnasHub.Infrastructure.Jobs;
using Xunit;

#endregion

namespace HarnasHub.Tests.Infrastructure.Jobs;

public class BackgroundJobQueueTests
{
	#region Public Methods

	[Fact]
	public void Should_deliver_jobs_in_order_on_their_own_lane_only()
	{
		var queue = new BackgroundJobQueue();
		var first = Guid.NewGuid();
		var second = Guid.NewGuid();
		var faceit = Guid.NewGuid();

		queue.Enqueue(first, JobLane.Demo);
		queue.Enqueue(faceit, JobLane.Faceit);
		queue.Enqueue(second, JobLane.Demo);

		Assert.True(queue.Reader(JobLane.Demo).TryRead(out var a));
		Assert.True(queue.Reader(JobLane.Demo).TryRead(out var b));
		Assert.False(queue.Reader(JobLane.Demo).TryRead(out _));
		Assert.True(queue.Reader(JobLane.Faceit).TryRead(out var c));
		Assert.Equal((first, second, faceit), (a, b, c));
	}

	[Fact]
	public async Task Should_wake_a_waiting_consumer()
	{
		var queue = new BackgroundJobQueue();
		var jobId = Guid.NewGuid();
		var waiting = queue.Reader(JobLane.Faceit).ReadAsync().AsTask();

		queue.Enqueue(jobId, JobLane.Faceit);

		Assert.Equal(jobId, await waiting.WaitAsync(TimeSpan.FromSeconds(5)));
	}

	#endregion
}
