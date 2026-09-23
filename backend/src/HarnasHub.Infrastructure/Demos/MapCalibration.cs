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

	// pos_x/pos_y = world coordinate of the overview image's top-left corner; scale = world units per pixel of a
	// 1024x1024 source image. de_dust2 is the only current-pool map exported "rotated" (rotate=1 in its overview
	// file), which swaps and mirrors the axes — unlike the rest, this is only verified against the documented
	// convention, not against a real Dust2 demo (none was available to test).
	private static readonly Dictionary<MapName, (float PosX, float PosY, float Scale, bool Rotated)> Calibrations = new()
	{
		[MapName.Dust2] = (-2476, 3239, 4.4f, true),
		[MapName.Mirage] = (-3230, 1713, 5.0f, false),
		[MapName.Inferno] = (-2087, 3870, 4.9f, false),
		[MapName.Nuke] = (-3453, 2887, 7.0f, false),
		[MapName.Ancient] = (-2953, 2164, 5.0f, false),
		[MapName.Anubis] = (-2796, 3328, 5.22f, false),
		[MapName.Cache] = (-2000, 3250, 5.5f, false),
	};

	// The radar images under frontend/public/maps aren't all the plain 1024x1024 overview: most are tighter,
	// non-square crops of it (e.g. ancient.webp is 1290x1467), and the browser stretches whatever it's given to fill
	// the radar box. A fraction that is correct in overview space therefore lands somewhere else entirely once
	// rendered — which is what put death dots a callout or two away from where the player actually died. Each entry
	// below is the sub-rectangle of the overview (in overview fractions) that the shipped image shows, so the result
	// stays image-relative, the same convention the coach's own map/nade/tactic markers use.
	//
	// Fitted by maximising the overlap between a real demo's player-position cloud and the image's rendered geometry,
	// then verified by eye against the drawn dots. Only maps a demo was available for are listed; Cache and Anubis
	// ship the untouched overview (1024x1024 and a 2x copy of it) and verified correct as-is, while Dust2, Inferno
	// and Nuke are cropped the same way but have no demo to fit against yet — they need one before they can be added.
	private static readonly Dictionary<MapName, (float Left, float Top, float Width, float Height)> RadarImageCrops = new()
	{
		[MapName.Ancient] = (0.0785f, 0.034f, 0.820f, 0.9325f),
		[MapName.Mirage] = (0.0815f, 0.127f, 0.862f, 0.7503f),
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

	/// <summary>Converts a world (X, Y) into a fraction in [0,1] of the radar image the app actually displays, or null
	/// if there's no calibration for this map.</summary>
	public static (float X, float Y)? ToRadarFraction(MapName mapName, float worldX, float worldY)
	{
		if (!Calibrations.TryGetValue(mapName, out var calibration))
		{
			return null;
		}

		var (pixelX, pixelY) = calibration.Rotated
			? ((worldY - calibration.PosY) / -calibration.Scale, (worldX - calibration.PosX) / calibration.Scale)
			: ((worldX - calibration.PosX) / calibration.Scale, (calibration.PosY - worldY) / calibration.Scale);

		var overviewX = pixelX / ImageSizePixels;
		var overviewY = pixelY / ImageSizePixels;

		if (RadarImageCrops.TryGetValue(mapName, out var crop))
		{
			overviewX = (overviewX - crop.Left) / crop.Width;
			overviewY = (overviewY - crop.Top) / crop.Height;
		}

		return (Math.Clamp(overviewX, 0f, 1f), Math.Clamp(overviewY, 0f, 1f));
	}

	#endregion
}
