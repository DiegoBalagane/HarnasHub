#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class OpponentCtFactsExtractorTests
{
	#region Public Methods

	[Fact]
	public void Should_return_no_post_plant_behaviour_without_a_plant()
	{
		Assert.Null(OpponentCtFactsExtractor.PostPlant(Round(5, MapSide.CT, MapSide.CT), Them.ToHashSet(), []));
	}

	[Fact]
	public void Should_detect_a_retake_when_they_fight_after_the_plant()
	{
		var round = Round(5, MapSide.CT, MapSide.CT, Plant(DemoBombSite.A, 60f));
		var kills = new[] { Kill(5, 11, 21, 70f, theirSide: MapSide.CT) };

		Assert.Equal(PostPlantBehaviour.Retake, OpponentCtFactsExtractor.PostPlant(round, Them.ToHashSet(), kills));
	}

	[Fact]
	public void Should_detect_a_save_when_survivors_do_not_engage_before_the_round_ends()
	{
		var round = Round(5, MapSide.CT, MapSide.T, Plant(DemoBombSite.A, 60f));
		var kills = new[]
		{
			Kill(5, 21, 11, 30f, theirSide: MapSide.CT),
			Kill(5, 21, 12, 200f, theirSide: MapSide.CT)
		};

		Assert.Equal(PostPlantBehaviour.Save, OpponentCtFactsExtractor.PostPlant(round, Them.ToHashSet(), kills));
	}

	[Fact]
	public void Should_report_all_dead_when_nobody_was_left_at_the_plant()
	{
		var round = Round(5, MapSide.CT, MapSide.T, Plant(DemoBombSite.B, 60f));
		var kills = Them.Select((id, i) => Kill(5, 21, id, 10f + i, theirSide: MapSide.CT)).ToArray();

		Assert.Equal(PostPlantBehaviour.AllDead, OpponentCtFactsExtractor.PostPlant(round, Them.ToHashSet(), kills));
	}

	#endregion
}
