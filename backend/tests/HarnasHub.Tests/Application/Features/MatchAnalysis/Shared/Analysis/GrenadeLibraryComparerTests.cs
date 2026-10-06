#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Application.Features.Tactics;
using Xunit;
using static HarnasHub.Tests.Application.Features.MatchAnalysis.MatchTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

public class GrenadeLibraryComparerTests
{
	#region Public Methods

	[Fact]
	public void Should_return_null_for_an_empty_library()
	{
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)]);

		Assert.Null(GrenadeLibraryComparer.Compare(context, []));
	}

	[Fact]
	public void Should_match_by_type_and_landing_distance_and_list_the_never_thrown()
	{
		var thrown = new[] { DemoTimelineFactory.Grenade(1, 1, DemoGrenadeType.Smoke) };
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], grenades: thrown);
		LibraryNade[] library =
		[
			new(Guid.NewGuid(), GrenadeType.Smoke, "Near", 0.51f, 0.6f),
			new(Guid.NewGuid(), GrenadeType.Smoke, "Far", 0.9f, 0.9f),
			new(Guid.NewGuid(), GrenadeType.Flash, "Wrong type", 0.5f, 0.6f)
		];

		var result = GrenadeLibraryComparer.Compare(context, library)!;

		Assert.Equal(3, result.TrainedTotal);
		Assert.Equal(1, result.TrainedThrown);
		Assert.Equal(1, result.Library.Single(n => n.Title == "Near").TimesThrown);
		Assert.Equal(0, result.Library.Single(n => n.Title == "Far").TimesThrown);
		Assert.Empty(result.Candidates);
	}

	[Fact]
	public void Should_suggest_repeated_unlisted_grenades_as_candidates()
	{
		var thrown = new[]
		{
			DemoTimelineFactory.Grenade(1, 1, DemoGrenadeType.Flash),
			DemoTimelineFactory.Grenade(2, 2, DemoGrenadeType.Flash),
			DemoTimelineFactory.Grenade(3, 3, DemoGrenadeType.Decoy)
		};
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], grenades: thrown);
		LibraryNade[] library = [new(Guid.NewGuid(), GrenadeType.Smoke, "Elsewhere", 0.1f, 0.1f)];

		var result = GrenadeLibraryComparer.Compare(context, library)!;

		var candidate = Assert.Single(result.Candidates);
		Assert.Equal("Flash", candidate.Type);
		Assert.Equal(2, candidate.Count);
		Assert.Equal(2, result.OurGrenadesThrown);
	}

	[Fact]
	public void Should_ignore_grenades_thrown_by_opponents()
	{
		var thrown = new[] { DemoTimelineFactory.Grenade(1, 1) with { ThrowerSteamId64 = 3 } };
		var context = AnalyzerTestSupport.Context([Round(1, MapSide.T)], grenades: thrown);

		var result = GrenadeLibraryComparer.Compare(context, [new(Guid.NewGuid(), GrenadeType.Smoke, "N", 0.5f, 0.6f)])!;

		Assert.Equal(0, result.TrainedThrown);
	}

	#endregion
}
