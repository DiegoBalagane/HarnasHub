using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class IndividualSignalTests
{
	#region Public Methods

	[Fact]
	public void Weight_should_shrink_with_team_games_on_the_map_and_overall()
	{
		Assert.Equal(1, IndividualSignal.Weight(0, 0, 7));
		Assert.Equal(0.5, IndividualSignal.Weight(5, 5, 7));
		Assert.Equal(0.5, IndividualSignal.Weight(0, 35, 7));
		Assert.Equal(0.2, IndividualSignal.Weight(20, 40, 7), 6);
	}

	[Fact]
	public void BlendPreference_should_keep_the_team_value_without_usable_comfort()
	{
		Assert.Equal(0.9, IndividualSignal.BlendPreference(0.9, 3, 10, 7, null));
		Assert.Equal(0.9, IndividualSignal.BlendPreference(0.9, 3, 10, 7, Comfort(rated: 2, regulars: 2, avoiding: 0)));
	}

	[Fact]
	public void BlendPreference_should_follow_solo_comfort_without_team_games()
	{
		var avoided = Comfort(rated: 5, regulars: 0, avoiding: 4);
		var favourite = Comfort(rated: 5, regulars: 5, avoiding: 0, share: 0.3, smoothed: 0.6);

		Assert.Equal(0, IndividualSignal.BlendPreference(0, 0, 0, 7, avoided));
		Assert.Equal(0.9, IndividualSignal.BlendPreference(0, 0, 0, 7, favourite), 6);
	}

	[Fact]
	public void WinRatePrior_should_be_capped_and_neutral_without_enough_regulars()
	{
		Assert.Equal(0.6, IndividualSignal.WinRatePrior(Comfort(5, 4, 0, smoothed: 0.75)));
		Assert.Equal(0.4, IndividualSignal.WinRatePrior(Comfort(5, 4, 0, smoothed: 0.2)));
		Assert.Equal(0.5, IndividualSignal.WinRatePrior(Comfort(5, 1, 0, smoothed: 0.75)));
		Assert.Equal(0.5, IndividualSignal.WinRatePrior(null));
	}

	[Fact]
	public void Advantage_priors_should_move_a_small_sample_but_barely_a_large_one()
	{
		var small = MapAdvantage.Advantage(1, 2, 1, 2, ourPrior: 0.6) - MapAdvantage.Advantage(1, 2, 1, 2);
		var large = MapAdvantage.Advantage(15, 30, 15, 30, ourPrior: 0.6) - MapAdvantage.Advantage(15, 30, 15, 30);

		Assert.InRange(small, 0.07, 0.072);
		Assert.InRange(large, 0.014, 0.015);
	}

	#endregion

	#region Internal Methods

	/// <summary>A comfort row with the given counts; win rates set to <paramref name="smoothed"/>.</summary>
	internal static MapComfort Comfort(int rated, int regulars, int avoiding, double share = 0.2, double? smoothed = 0.55, MapName map = MapName.Mirage) =>
		new(map, rated, regulars, avoiding, share, smoothed, smoothed, 1.1, [], []);

	#endregion
}
