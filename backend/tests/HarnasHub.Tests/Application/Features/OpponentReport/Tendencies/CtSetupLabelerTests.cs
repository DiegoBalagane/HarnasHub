#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class CtSetupLabelerTests
{
	#region Public Methods

	[Fact]
	public void Should_label_a_setup_by_players_per_area()
	{
		MapArea?[] areas = [MapArea.A, MapArea.A, MapArea.Mid, MapArea.B, MapArea.B];

		Assert.Equal("2A-1M-2B", CtSetupLabeler.Label(areas));
		Assert.Null(CtSetupLabeler.Stack(areas));
	}

	[Fact]
	public void Should_not_label_setups_with_too_few_known_spots()
	{
		Assert.Null(CtSetupLabeler.Label([MapArea.A, null, null, MapArea.B]));
	}

	[Fact]
	public void Should_detect_a_stack_of_three_on_one_site()
	{
		Assert.Equal(MapArea.B, CtSetupLabeler.Stack([MapArea.B, MapArea.B, MapArea.B, MapArea.A]));
		Assert.Equal(MapArea.A, CtSetupLabeler.Stack([MapArea.A, MapArea.A, MapArea.A, MapArea.Mid]));
	}

	[Theory]
	[InlineData("1A-1M-3B", MapArea.A)]
	[InlineData("3A-0M-2B", MapArea.B)]
	[InlineData("2A-1M-2B", null)]
	[InlineData("garbage", null)]
	public void Should_find_the_weaker_site(string label, MapArea? expected)
	{
		Assert.Equal(expected, CtSetupLabeler.WeakerSite(label));
	}

	#endregion
}
