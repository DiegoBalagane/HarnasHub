#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Faceit;
using Microsoft.Extensions.Logging.Abstractions;

#endregion

namespace HarnasHub.Tests.Common;

/// <summary>Builds a real <see cref="FaceitDemoMatchLookup"/> over a test FACEIT client (unconfigured by default, i.e. no prefill).</summary>
public static class TestFaceitLookup
{
	#region Public Methods

	/// <summary>A lookup over <paramref name="client"/>, or over an unconfigured client when null.</summary>
	public static FaceitDemoMatchLookup Create(IApplicationDbContext dbContext, TestFaceitClient? client = null) =>
		new(client ?? new TestFaceitClient(isConfigured: false), dbContext, NullLogger<FaceitDemoMatchLookup>.Instance);

	#endregion
}
