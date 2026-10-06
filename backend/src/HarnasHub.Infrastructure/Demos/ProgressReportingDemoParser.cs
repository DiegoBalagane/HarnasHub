#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Decorates <see cref="DemoFileParser"/> so every parse reports its progress (bytes read of the demo stream) to the
/// current <see cref="IJobProgress"/> — handlers get demo-analysis progress without knowing about it, and outside a job the
/// reports are no-ops.</summary>
public sealed class ProgressReportingDemoParser(DemoFileParser inner, IJobProgress jobProgress) : IDemoParser
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		var result = await inner.ParseAsync(Wrap(demoStream), cancellationToken);
		jobProgress.ReportStep(1);
		return result;
	}

	/// <inheritdoc />
	public async Task<DemoTimeline> ParseAsync(Stream demoStream, DemoParseOptions options, CancellationToken cancellationToken)
	{
		var timeline = await inner.ParseAsync(Wrap(demoStream), options, cancellationToken);
		jobProgress.ReportStep(1);
		return timeline;
	}

	#endregion

	#region Private Methods

	private Stream Wrap(Stream demoStream) => new ProgressReportingStream(demoStream, jobProgress.ReportStep);

	#endregion
}
