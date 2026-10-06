#region Usings

using HarnasHub.Application.Features.MatchAnalysis.Shared;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared;

public class BuyTypeClassifierTests
{
	#region Public Methods

	[Theory]
	[InlineData(1, true)]
	[InlineData(13, true)]
	[InlineData(2, false)]
	[InlineData(12, false)]
	[InlineData(25, false)]
	[InlineData(28, false)]
	public void Should_flag_only_regulation_half_openers_as_pistol_rounds(int round, bool expected)
	{
		Assert.Equal(expected, BuyTypeClassifier.IsPistolRound(round));
	}

	[Theory]
	[InlineData(4000, BuyType.Eco)]
	[InlineData(4999, BuyType.Eco)]
	[InlineData(5000, BuyType.SemiEco)]
	[InlineData(12499, BuyType.SemiEco)]
	[InlineData(12500, BuyType.Force)]
	[InlineData(19499, BuyType.Force)]
	[InlineData(19500, BuyType.Full)]
	[InlineData(30000, BuyType.Full)]
	public void Should_classify_a_five_man_team_by_total_equipment_value(int total, BuyType expected)
	{
		Assert.Equal(expected, BuyTypeClassifier.Classify(5, total, 5));
	}

	[Fact]
	public void Should_judge_a_short_handed_team_by_its_per_player_average()
	{
		// 4 players at $4000 each is a full buy even though the total is below the 5-man threshold.
		Assert.Equal(BuyType.Full, BuyTypeClassifier.Classify(5, 16000, 4));
	}

	[Fact]
	public void Should_call_a_pistol_round_pistol_whatever_was_bought()
	{
		Assert.Equal(BuyType.Pistol, BuyTypeClassifier.Classify(13, 25000, 5));
	}

	[Fact]
	public void Should_not_divide_by_zero_without_players()
	{
		Assert.Equal(BuyType.Eco, BuyTypeClassifier.Classify(5, 0, 0));
	}

	#endregion
}
