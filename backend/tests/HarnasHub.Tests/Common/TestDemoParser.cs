using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IDemoParser"/> returning a fixed result (or throwing) instead of reading a real demo file.</summary>
public class TestDemoParser(DemoParseResult? result = null, Exception? throwOnParse = null) : IDemoParser
{
	#region Public Methods

	public Task<DemoParseResult> ParseAsync(Stream demoStream, CancellationToken cancellationToken)
	{
		if (throwOnParse is not null)
		{
			throw throwOnParse;
		}

		return Task.FromResult(result ?? new DemoParseResult(0, null, []));
	}

	#endregion
}
