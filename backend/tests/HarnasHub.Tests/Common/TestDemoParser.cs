#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IDemoParser"/> returning a fixed result/timeline (or throwing) instead of reading a real demo file.</summary>
public class TestDemoParser(DemoParseResult? result = null, Exception? throwOnParse = null, DemoTimeline? timeline = null) : IDemoParser
{
	#region Public Properties

	/// <summary>Options passed to the last timeline parse, so tests can check callers only enable what they need.</summary>
	public DemoParseOptions? LastOptions { get; private set; }

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		if (throwOnParse is not null)
		{
			throw throwOnParse;
		}

		return Task.FromResult(result ?? new DemoParseResult(0, null, [], []));
	}

	/// <inheritdoc />
	public Task<DemoTimeline> ParseAsync(Stream demoStream, DemoParseOptions options, CancellationToken cancellationToken)
	{
		LastOptions = options;

		if (throwOnParse is not null)
		{
			throw throwOnParse;
		}

		return Task.FromResult(timeline ?? new DemoTimeline(null, null, false, result, [], []));
	}

	#endregion
}
