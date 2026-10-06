#region Usings

using ErrorOr;
using HarnasHub.Application.Common.Jobs;
using HarnasHub.Tests.Common;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Jobs;

public class JobRunnerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_the_definitions_outcome()
	{
		var outcome = await Run(new FakeDefinition((_, _) => Task.FromResult(JobOutcome.Success("{}"))), CancellationToken.None);

		Assert.True(outcome.Succeeded);
	}

	[Fact]
	public async Task Should_fail_with_a_generic_polish_message_on_an_unexpected_exception()
	{
		var outcome = await Run(new FakeDefinition((_, _) => throw new InvalidOperationException("boom")), CancellationToken.None);

		Assert.Equal(JobOutcome.UnexpectedErrorMessage, outcome.ErrorMessage);
	}

	[Fact]
	public async Task Should_fail_with_the_timeout_message_when_the_time_limit_passes()
	{
		var definition = new FakeDefinition(async (_, token) =>
		{
			await Task.Delay(Timeout.Infinite, token);
			return JobOutcome.Success("{}");
		}, TimeSpan.FromMilliseconds(50));

		var outcome = await Run(definition, CancellationToken.None);

		Assert.Equal(JobOutcome.TimeoutMessage, outcome.ErrorMessage);
	}

	[Fact]
	public async Task Should_fail_with_the_shutdown_message_when_the_server_stops()
	{
		using var stopping = new CancellationTokenSource();
		var definition = new FakeDefinition(async (_, token) =>
		{
			await stopping.CancelAsync();
			await Task.Delay(Timeout.Infinite, token);
			return JobOutcome.Success("{}");
		});

		var outcome = await Run(definition, stopping.Token);

		Assert.Equal(JobOutcome.ShutdownMessage, outcome.ErrorMessage);
	}

	#endregion

	#region Private Methods

	private static Task<JobOutcome> Run(IJobDefinition definition, CancellationToken stoppingToken) =>
		JobRunner.ExecuteAsync(definition, "{}", new TestSender<int>(0), NullLogger.Instance, stoppingToken);

	#endregion

	#region Nested Types

	private sealed class FakeDefinition(Func<string, CancellationToken, Task<JobOutcome>> execute, TimeSpan? timeout = null) : IJobDefinition
	{
		public string Kind => "test";

		public Type RequestType => typeof(object);

		public JobLane Lane => JobLane.Demo;

		public string InitialStage => "Start";

		public TimeSpan Timeout { get; } = timeout ?? TimeSpan.FromMinutes(1);

		public Task<List<Error>> ValidateAsync(object request, IServiceProvider services, CancellationToken cancellationToken) =>
			Task.FromResult(new List<Error>());

		public string SerializePayload(object request) => "{}";

		public Task<JobOutcome> ExecuteAsync(ISender sender, string payloadJson, CancellationToken cancellationToken) =>
			execute(payloadJson, cancellationToken);
	}

	#endregion
}
