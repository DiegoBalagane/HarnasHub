#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Infrastructure.Demos;

/// <summary>Converts a demo's raw world coordinates into the same radar-relative [0,1] fraction convention used by
/// <c>MapPositionAssignment.X/Y</c>, using each map's official overview calibration (pos_x/pos_y/scale from the game's
/// own overview files — the same values every CS radar/heatmap tool uses, source: github.com/MurkyYT/cs2-map-icons).</summary>
public static class MapCalibration
{
	#region Private Fields

	// pos_x/pos_y = world coordinate of the overview image's top-left corner; scale = world units per pixel of a 1024x1024 source
	// image — checked against the game's own resource/overviews/*.txt. de_dust2 says rotate=1, but in CS2 its radar is drawn in the
	// plain orientation (the same file puts T spawn at the bottom, CT at the top), so every map uses the same formula.
	private static readonly Dictionary<MapName, (float PosX, float PosY, float Scale)> Calibrations = new()
	{
		[MapName.Dust2] = (-2476, 3239, 4.4f),
		[MapName.Mirage] = (-3230, 1713, 5.0f),
		[MapName.Inferno] = (-2087, 3870, 4.9f),
		[MapName.Nuke] = (-3453, 2887, 7.0f),
		[MapName.Ancient] = (-2953, 2164, 5.0f),
		[MapName.Anubis] = (-2796, 3328, 5.22f),
		[MapName.Cache] = (-2000, 3250, 5.5f),
	};

	// The radar images under frontend/public/maps aren't all the plain 1024x1024 overview: most are tighter,
	// non-square crops of it (e.g. ancient.webp is 1290x1467), and the browser stretches whatever it's given to fill
	// the radar box. A fraction that is correct in overview space therefore lands somewhere else entirely once
	// rendered — which is what put death dots a callout or two away from where the player actually died. Each entry
	// below is the sub-rectangle of the overview (in overview fractions) that the shipped image shows, so the result
	// stays image-relative, the same convention the coach's own map/nade/tactic markers use.
	//
	// Registered against the official radar images extracted from the game files (panorama/images/overheadmaps/*_radar_psd):
	// L/T/W/H maximise the overlap (IoU 0.96-0.99) of the drawn floor of both images. The shipped images turned out to be
	// ~1.6x upscaled crops of the official radars (uniform scale), and on real demos 97-99 % of positions now land on drawn
	// floor (Nuke 87 % → 98 %, Mirage 93 % → 99 %, Anubis 91 % → 97 %). Cache ships the untouched 1024px overview. Nuke's
	// image only shows the upper level, so lower-level positions are drawn over the upper plan.
	private static readonly Dictionary<MapName, (float Left, float Top, float Width, float Height)> RadarImageCrops = new()
	{
		[MapName.Ancient] = (0.0933f, 0.0462f, 0.787f, 0.895f),
		[MapName.Mirage] = (0.0922f, 0.1338f, 0.84f, 0.7311f),
		[MapName.Nuke] = (0.04f, 0.2536f, 0.9536f, 0.519f),
		[MapName.Dust2] = (0.041f, 0.0079f, 0.9264f, 0.9893f),
		[MapName.Inferno] = (0.049f, 0.032f, 0.9152f, 0.9097f),
		[MapName.Anubis] = (0.014f, 0.003f, 0.9805f, 0.9805f),
	};

	private const float ImageSizePixels = 1024f;

	#endregion

	#region Public Methods

	/// <summary>Best-effort mapping from a demo's raw map string (e.g. "de_mirage") to the app's current map pool; null if unrecognized.</summary>
	public static MapName? ParseMapName(string rawMapName)
	{
		var normalized = rawMapName.Replace("de_", "", StringComparison.OrdinalIgnoreCase).Trim();

		foreach (var mapName in Calibrations.Keys)
		{
			if (string.Equals(mapName.ToString(), normalized, StringComparison.OrdinalIgnoreCase))
			{
				return mapName;
			}
		}

		return null;
	}

	/// <summary>Whether world coordinates on this map can be converted to radar fractions at all.</summary>
	public static bool IsCalibrated(MapName mapName) => Calibrations.ContainsKey(mapName);

	/// <summary>Converts a world (X, Y) into a fraction in [0,1] of the radar image the app actually displays, or null
	/// if there's no calibration for this map.</summary>
	public static (float X, float Y)? ToRadarFraction(MapName mapName, float worldX, float worldY)
	{
		if (Overview(mapName) is not { } toOverview)
		{
			return null;
		}

		var (overviewX, overviewY) = toOverview(worldX, worldY);

		if (RadarImageCrops.TryGetValue(mapName, out var crop))
		{
			overviewX = (overviewX - crop.Left) / crop.Width;
			overviewY = (overviewY - crop.Top) / crop.Height;
		}

		return (Math.Clamp(overviewX, 0f, 1f), Math.Clamp(overviewY, 0f, 1f));
	}

	/// <summary>World (X, Y) → fraction of the full 1024px overview, before any image crop and unclamped — the space a crop
	/// is fitted in (see the dev-only position dump/fit helpers); null if there's no calibration for this map.</summary>
	public static Func<float, float, (float X, float Y)>? Overview(MapName mapName)
	{
		if (!Calibrations.TryGetValue(mapName, out var calibration))
		{
			return null;
		}

		return (worldX, worldY) =>
		{
			var pixelX = (worldX - calibration.PosX) / calibration.Scale;
			var pixelY = (calibration.PosY - worldY) / calibration.Scale;
			return (pixelX / ImageSizePixels, pixelY / ImageSizePixels);
		};
	}

	#endregion
}
