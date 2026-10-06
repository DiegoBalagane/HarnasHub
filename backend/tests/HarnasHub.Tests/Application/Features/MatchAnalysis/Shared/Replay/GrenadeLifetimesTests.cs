#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Replay;

public class GrenadeLifetimesTests
{
	#region Public Methods

	[Fact]
	public void Should_use_the_recorded_detonation_and_the_nominal_effect_length()
	{
		var molotov = Grenade(1, 1, Them[0], SiteA, seconds: 30f, type: DemoGrenadeType.Molotov) with { DetonationSecond = 31.5f };

		var (detonate, end, approximate) = GrenadeLifetimes.Resolve(molotov, 115f);

		Assert.Equal(31.5f, detonate);
		Assert.Equal(38.5f, end);
		Assert.False(approximate);
	}

	[Theory]
	[InlineData(DemoGrenadeType.Smoke, 12.5f)]
	[InlineData(DemoGrenadeType.Flash, 11.6f)]
	[InlineData(DemoGrenadeType.HighExplosive, 11.6f)]
	public void Should_fall_back_to_a_typical_fuse_when_the_detonation_was_not_recorded(DemoGrenadeType type, float expected)
	{
		var grenade = Grenade(1, 1, Them[0], SiteA, seconds: 10f, type: type);

		var (detonate, _, approximate) = GrenadeLifetimes.Resolve(grenade, 115f);

		Assert.Equal(expected, detonate, 3);
		Assert.True(approximate);
	}

	[Fact]
	public void Should_cap_the_effect_at_the_end_of_the_round()
	{
		var smoke = Grenade(1, 1, Them[0], SiteA, seconds: 100f) with { DetonationSecond = 102f };

		var (_, end, _) = GrenadeLifetimes.Resolve(smoke, 110f);

		Assert.Equal(110f, end);
	}

	[Fact]
	public void Should_treat_a_detonation_before_the_throw_as_unrecorded()
	{
		var smoke = Grenade(1, 1, Them[0], SiteA, seconds: 20f) with { DetonationSecond = 5f };

		var (detonate, _, approximate) = GrenadeLifetimes.Resolve(smoke, 115f);

		Assert.True(approximate);
		Assert.Equal(22.5f, detonate);
	}

	#endregion
}
