#region Usings

using HarnasHub.Application.Common.Maps;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Pure "which part of the map" lookup on top of <see cref="MapZones"/>: a point inside the A/B/Mid polygon is that
/// area, a point in T spawn is no area (nobody has committed anywhere yet), and any other point (a callout, CT spawn, a
/// corridor between zones) goes to the nearest A/B/Mid polygon centre — so "A Ramp" counts as A and "Apartments" as B
/// without a hand-written callout table. Maps without zones resolve nothing.</summary>
public static class MapAreaResolver
{
	#region Public Methods

	/// <summary>The area of a radar point, or null (no zones, T spawn, or no position).</summary>
	public static MapArea? Resolve(MapName? map, float? radarX, float? radarY)
	{
		if (map is not { } m || radarX is not { } x || radarY is not { } y)
		{
			return null;
		}

		var zone = MapZones.Find(m, x, y);
		switch (zone?.Kind)
		{
			case MapZoneKind.BombsiteA:
				return MapArea.A;
			case MapZoneKind.BombsiteB:
				return MapArea.B;
			case MapZoneKind.Mid:
				return MapArea.Mid;
			case MapZoneKind.TSpawn:
				return null;
		}

		return Centres(m)
			.OrderBy(c => Distance(c.X, c.Y, x, y))
			.Select(c => (MapArea?)c.Area)
			.FirstOrDefault();
	}

	/// <summary>Centre of the T spawn polygon (start of the entry arrows), or null when the map has no zones.</summary>
	public static (float X, float Y)? TSpawnCentre(MapName map) =>
		MapZones.For(map).FirstOrDefault(z => z.Kind == MapZoneKind.TSpawn) is { } spawn ? Centre(spawn) : null;

	/// <summary>Short Polish label of an area for suggestion texts.</summary>
	public static string Label(MapArea area) => area switch
	{
		MapArea.A => "A",
		MapArea.B => "B",
		_ => "Mid"
	};

	#endregion

	#region Private Methods

	private static IEnumerable<(MapArea Area, float X, float Y)> Centres(MapName map)
	{
		foreach (var zone in MapZones.For(map))
		{
			MapArea? area = zone.Kind switch
			{
				MapZoneKind.BombsiteA => MapArea.A,
				MapZoneKind.BombsiteB => MapArea.B,
				MapZoneKind.Mid => MapArea.Mid,
				_ => null
			};

			if (area is { } a)
			{
				var (cx, cy) = Centre(zone);
				yield return (a, cx, cy);
			}
		}
	}

	private static (float X, float Y) Centre(MapZone zone) =>
		(zone.Polygon.Average(p => p.X), zone.Polygon.Average(p => p.Y));

	private static float Distance(float x1, float y1, float x2, float y2) =>
		MathF.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));

	#endregion
}
