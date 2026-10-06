#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Common.Maps;

/// <summary>What a zone is, so code can ask "was this on a bombsite" without knowing callout names.</summary>
public enum MapZoneKind
{
	BombsiteA = 0,
	BombsiteB = 1,
	Mid = 2,
	TSpawn = 3,
	CTSpawn = 4,
	Callout = 5
}

/// <summary>A named map area as a polygon of radar fractions in [0,1] (same convention as <c>DemoPosition.RadarX/Y</c>).</summary>
public sealed record MapZone(string Name, MapZoneKind Kind, IReadOnlyList<(float X, float Y)> Polygon)
{
	/// <summary>Whether the radar point lies inside this zone's polygon (even-odd ray casting; edges count as inside
	/// only incidentally, which is irrelevant at the precision these polygons have).</summary>
	public bool Contains(float x, float y)
	{
		var inside = false;
		var count = Polygon.Count;

		for (int i = 0, j = count - 1; i < count; j = i++)
		{
			var (xi, yi) = Polygon[i];
			var (xj, yj) = Polygon[j];

			var crosses = (yi > y) != (yj > y) && x < (xj - xi) * (y - yi) / (yj - yi) + xi;
			if (crosses)
			{
				inside = !inside;
			}
		}

		return inside;
	}
}

/// <summary>Pure lookup of which named zone a radar point falls into, per map. Zones are hand-estimated (see
/// <see cref="MapZoneData"/>), so they are good enough for "which site / which part of the map" aggregates, not for
/// exact callouts. A map without zones simply resolves nothing — every caller must treat a null zone as "unknown".</summary>
public static class MapZones
{
	#region Public Methods

	/// <summary>Every zone defined for <paramref name="map"/>, bombsites and spawns first; empty when the map has none
	/// (or its positions can't be trusted to line up with the radar yet).</summary>
	public static IReadOnlyList<MapZone> For(MapName map) =>
		MapZoneData.Zones.TryGetValue(map, out var zones) && MapZoneData.PositionsAlignWithRadar(map) ? zones : [];

	/// <summary>Whether <paramref name="map"/> has usable zones.</summary>
	public static bool HasZones(MapName map) => For(map).Count > 0;

	/// <summary>The first zone containing the point (bombsites and spawns win over broader callouts), or null.</summary>
	public static MapZone? Find(MapName map, float radarX, float radarY) =>
		For(map).FirstOrDefault(zone => zone.Contains(radarX, radarY));

	/// <summary>Same as <see cref="Find(MapName, float, float)"/>, tolerating a missing map or position.</summary>
	public static MapZone? Find(MapName? map, float? radarX, float? radarY) =>
		map is { } m && radarX is { } x && radarY is { } y ? Find(m, x, y) : null;

	#endregion
}
