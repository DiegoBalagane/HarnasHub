#region Usings

using HarnasHub.Application.Features.OpponentReport.Tendencies;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.Tendencies;

public class MapAreaResolverTests
{
	#region Public Methods

	[Theory]
	[InlineData(0.53f, 0.82f, MapArea.A)]
	[InlineData(0.15f, 0.22f, MapArea.B)]
	[InlineData(0.57f, 0.44f, MapArea.Mid)]
	public void Should_resolve_points_inside_the_main_zones(float x, float y, MapArea expected)
	{
		Assert.Equal(expected, MapAreaResolver.Resolve(MapName.Mirage, x, y));
	}

	[Fact]
	public void Should_return_null_in_t_spawn_without_zones_or_without_a_position()
	{
		Assert.Null(MapAreaResolver.Resolve(MapName.Mirage, 0.94f, 0.32f));
		Assert.Null(MapAreaResolver.Resolve(MapName.Inferno, 0.5f, 0.5f));
		Assert.Null(MapAreaResolver.Resolve(MapName.Mirage, null, 0.5f));
		Assert.Null(MapAreaResolver.Resolve(null, 0.5f, 0.5f));
	}

	[Fact]
	public void Should_assign_a_callout_outside_the_main_zones_to_the_nearest_one()
	{
		// Mirage "Palace" (pixels ~1057,1017) is a callout outside every main zone, nearest to A.
		Assert.Equal(MapArea.A, MapAreaResolver.Resolve(MapName.Mirage, 1057f / 1374, 1017f / 1196));
	}

	[Fact]
	public void Should_locate_the_t_spawn_centre_only_on_maps_with_zones()
	{
		Assert.NotNull(MapAreaResolver.TSpawnCentre(MapName.Mirage));
		Assert.Null(MapAreaResolver.TSpawnCentre(MapName.Inferno));
	}

	#endregion
}
