using HarnasHub.Application.Features.Veto.Shared;
using HarnasHub.Core.Enums;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Veto.Shared;

public class VetoScoringFaceitTests
{
	#region Public Methods

	[Fact]
	public void Should_score_without_opponent_reasons_when_no_faceit_numbers_are_given()
	{
		var result = VetoScoring.Suggest([Input(MapName.Mirage)])[0];

		Assert.DoesNotContain(result.Reasons, r => r.Contains("rywala", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void Should_ignore_the_opponent_win_rate_below_the_minimum_sample()
	{
		var baseline = VetoScoring.Suggest([Input(MapName.Mirage)])[0].Score;

		var result = VetoScoring.Suggest([Input(MapName.Mirage) with { TheirGames = 3, TheirWins = 3, TheirWinRate = 0.9 }])[0];

		Assert.Equal(baseline, result.Score);
		Assert.Contains("Rywal: 3 mecze na tej mapie — za mało, by oceniać ich skuteczność", result.Reasons);
	}

	[Fact]
	public void Should_count_a_strong_opponent_with_enough_games_against_the_map()
	{
		var baseline = VetoScoring.Suggest([Input(MapName.Mirage)])[0].Score;

		var result = VetoScoring.Suggest([Input(MapName.Mirage) with { TheirGames = 10, TheirWins = 7, TheirWinRate = 0.7 }])[0];

		Assert.Equal(baseline - 12, result.Score);
		Assert.Contains("Przewaga rywala: ich 70% wygranych przy 10 meczach", result.Reasons);
		Assert.Contains("przewaga rywala: ich 70% przy 10 meczach", result.Note);
	}

	[Fact]
	public void Should_cap_the_opponent_contribution()
	{
		var baseline = VetoScoring.Suggest([Input(MapName.Nuke)])[0].Score;

		var result = VetoScoring.Suggest([Input(MapName.Nuke) with { TheirGames = 20, TheirWins = 1, TheirWinRate = 0.05 }])[0];

		Assert.Equal(baseline + VetoMapScorer.MaxOpponentPoints, result.Score);
		Assert.Contains(result.Reasons, r => r.StartsWith("Słabość rywala: ich 5%"));
	}

	[Fact]
	public void Should_add_our_faceit_team_games_to_our_record()
	{
		var result = VetoScoring.Suggest([Input(MapName.Inferno, wins: 1, losses: 1) with { OurFaceitGames = 4, OurFaceitWins = 3 }])[0];

		Assert.Contains("Nasz bilans 4-2 (67% wygranych w 6 meczach, w tym 4 na FACEIT)", result.Reasons);
	}

	#endregion

	#region Private Methods

	private static MapVetoInput Input(MapName map, int wins = 3, int losses = 3) =>
		new(map, MapPoolStatus.Playable, wins, losses, 0, 0, 0, 0, 0, 0);

	#endregion
}
