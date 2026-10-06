#region Usings

using HarnasHub.Application.Common.Maps;
using HarnasHub.Core.Enums;
using Xunit;

#endregion

namespace HarnasHub.Tests.Application.Common.Maps;

public class MapZonesTests
{
	#region Public Methods

	[Theory]
	[InlineData(0.5f, 0.5f, true)]
	[InlineData(0.2f, 0.8f, false)]
	[InlineData(0.05f, 0.5f, false)]
	[InlineData(0.5f, 0.95f, false)]
	public void Should_detect_points_inside_a_concave_polygon(float x, float y, bool expected)
	{
		// An L-shape: the notch at the bottom-left corner must not count as inside.
		var zone = new MapZone("L", MapZoneKind.Callout, [(0.1f, 0.1f), (0.9f, 0.1f), (0.9f, 0.9f), (0.4f, 0.9f), (0.4f, 0.6f), (0.1f, 0.6f)]);

		Assert.Equal(expected, zone.Contains(x, y));
	}

	[Fact]
	public void Should_resolve_mirage_bombsites_from_their_radar_markers()
	{
		// Centres of the orange plant markers on the shipped mirage.webp (1374x1196).
		Assert.Equal(MapZoneKind.BombsiteA, MapZones.Find(MapName.Mirage, 737f / 1374, 1030f / 1196)?.Kind);
		Assert.Equal(MapZoneKind.BombsiteB, MapZones.Find(MapName.Mirage, 225f / 1374, 242f / 1196)?.Kind);
	}

	[Fact]
	public void Should_return_null_outside_every_zone()
	{
		Assert.Null(MapZones.Find(MapName.Mirage, 0.99f, 0.99f));
	}

	[Fact]
	public void Should_have_no_zones_for_maps_whose_positions_do_not_align_with_the_radar_yet()
	{
		Assert.False(MapZones.HasZones(MapName.Dust2));
		Assert.Null(MapZones.Find(MapName.Inferno, 0.5f, 0.5f));
	}

	[Fact]
	public void Should_tolerate_a_missing_map_or_position()
	{
		Assert.Null(MapZones.Find(null, 0.5f, 0.5f));
		Assert.Null(MapZones.Find(MapName.Mirage, null, 0.5f));
	}

	[Fact]
	public void Should_list_bombsites_before_callouts()
	{
		var kinds = MapZones.For(MapName.Mirage).Select(z => z.Kind).ToList();

		Assert.Equal(kinds.OrderBy(k => k), kinds);
		Assert.Equal(MapZoneKind.BombsiteA, kinds[0]);
	}

	#endregion
}
