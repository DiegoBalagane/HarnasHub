using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.OpponentReport.Shared;

public class LifetimeMapCalculatorTests
{
	#region Public Methods

	[Fact]
	public void Should_sum_matches_average_kd_and_flag_maps_played_a_lot()
	{
		IReadOnlyList<FaceitLifetimeMapStats> first = [new("de_ancient", 60, 33, 1.2), new("Mirage", 70, 35, 1.0), new("de_nuke", 10, 4, 0.9)];
		IReadOnlyList<FaceitLifetimeMapStats> second = [new("Ancient", 53, 27, 1.04), new("de_mirage", 70, 36, 1.02), new("de_overpass", 99, 50, 1.5)];

		var lifetime = LifetimeMapCalculator.Calculate([first, second]);

		var ancient = lifetime[MapName.Ancient];
		Assert.Equal((2, 113, 60), (ancient.Players, ancient.Matches, ancient.Wins));
		Assert.Equal(1.12, ancient.AvgKdRatio!.Value, 6);
		Assert.Equal(113.0 / 263, ancient.Share, 6);
		Assert.True(ancient.IsExperienced);
		Assert.False(lifetime[MapName.Nuke].IsExperienced);
		Assert.Equal(0, lifetime[MapName.Dust2].Matches);
		Assert.Null(LifetimeMapCalculator.ToDto(lifetime[MapName.Dust2]));
		Assert.Equal(new MapLifetimeDto(2, 113, 53.1, 1.12, 43, true), LifetimeMapCalculator.ToDto(ancient));
	}

	[Fact]
	public void Should_only_nudge_a_preference_backed_by_team_games()
	{
		var lifetime = new MapLifetime(MapName.Ancient, 5, 113, 60, 1.1, 0.25);

		var noTeamData = LifetimeMapCalculator.Blend(0, 0, lifetime);
		var manyTeamGames = LifetimeMapCalculator.Blend(0, 18, lifetime);

		Assert.Equal(LifetimeMapCalculator.Preference(lifetime), noTeamData, 6);
		Assert.True(manyTeamGames < 0.1);
		Assert.Equal(0.7, LifetimeMapCalculator.Blend(0.7, 3, null));
	}

	#endregion
}
