#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class CtSideTendencyAggregatorTests
{
	#region Public Methods

	[Fact]
	public void Should_aggregate_setups_stacks_aggression_awp_and_post_plant()
	{
		MapArea?[] standard = [MapArea.A, MapArea.A, MapArea.Mid, MapArea.B, MapArea.B];
		MapArea?[] stackB = [MapArea.A, MapArea.B, MapArea.B, MapArea.B, MapArea.Mid];
		var awp = new AwpKillFact(0.5f, 0.4f, MapArea.Mid, 15f);
		var rounds = new[]
		{
			CtRound(13, standard, earlyKills: 1, postPlant: PostPlantBehaviour.Retake, won: true, awp),
			CtRound(14, standard, postPlant: PostPlantBehaviour.Save, won: false),
			CtRound(15, stackB, earlyKills: 2, postPlant: PostPlantBehaviour.AllDead, won: false, awp),
			CtRound(16, [MapArea.A, null, null, null, null])
		}.Select(r => new IndexedRound(0, r)).ToList();

		var ct = CtSideTendencyAggregator.Aggregate(rounds);

		Assert.Equal(4, ct.Rounds);
		Assert.Equal("2A-1M-2B", ct.Setups[0].Label);
		Assert.Equal(66.7, ct.Setups[0].Percent);
		Assert.Equal("B", Assert.Single(ct.Stacks).Label);
		Assert.Equal(20, ct.SetupPositions.Count);
		Assert.Equal("Mid", Assert.Single(ct.AwpAreas).Label);
		Assert.Equal(2, ct.AwpKillPositions.Count);
		Assert.Equal(2, ct.EarlyKillRounds);
		Assert.Equal(50, ct.EarlyKillPercent);
		Assert.Equal(2, ct.PostPlantRounds);
		Assert.Equal(1, ct.Retakes);
		Assert.Equal(1, ct.Saves);
		Assert.Equal(1, ct.RetakesWon);
	}

	[Fact]
	public void Should_return_an_empty_summary_without_ct_rounds()
	{
		var ct = CtSideTendencyAggregator.Aggregate([new IndexedRound(0, TRound(1, MapArea.A, 30f))]);

		Assert.Equal(0, ct.Rounds);
		Assert.Empty(ct.Setups);
		Assert.Equal(0, ct.EarlyKillPercent);
	}

	#endregion
}
