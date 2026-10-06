using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.Shared;

public class VetoScoringFaceitTests
{
	#region Public Methods

	[Fact]
	public void Should_score_exactly_as_before_without_a_faceit_advantage()
	{
		var withoutFaceit = VetoScoring.Suggest([Input(MapName.Mirage)]);
		var withNullFaceit = VetoScoring.Suggest([Input(MapName.Mirage) with { FaceitAdvantage = null }]);

		Assert.Equal(withoutFaceit[0].Score, withNullFaceit[0].Score);
		Assert.DoesNotContain(withoutFaceit[0].Reasons, r => r.StartsWith("FACEIT"));
	}

	[Fact]
	public void Should_weight_the_advantage_by_confidence()
	{
		var baseline = VetoScoring.Suggest([Input(MapName.Mirage)])[0].Score;

		var low = VetoScoring.Suggest([Input(MapName.Mirage) with { FaceitAdvantage = 0.2, FaceitMinGames = 1 }])[0];
		var high = VetoScoring.Suggest([Input(MapName.Mirage) with { FaceitAdvantage = 0.2, FaceitMinGames = 12 }])[0];

		Assert.Equal(baseline + 5, low.Score);
		Assert.Equal(baseline + 16, high.Score);
		Assert.Contains("FACEIT: przewaga +20 pp nad przeciwnikiem (pewność wysoka)", high.Reasons);
	}

	[Fact]
	public void Should_cap_the_faceit_contribution()
	{
		var baseline = VetoScoring.Suggest([Input(MapName.Nuke)])[0].Score;

		var result = VetoScoring.Suggest([Input(MapName.Nuke) with { FaceitAdvantage = -0.6, FaceitMinGames = 20 }])[0];

		Assert.Equal(baseline - VetoScoring.MaxFaceitPoints, result.Score);
	}

	#endregion

	#region Private Methods

	private static MapVetoInput Input(MapName map) =>
		new(map, MapPoolStatus.Playable, 3, 3, 0, 0, 0, 0, 0, 0);

	#endregion
}
