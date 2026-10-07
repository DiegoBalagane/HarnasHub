#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Common.Maps;

/// <summary>Which maps' stored radar fractions can be trusted on the images the app ships. Every pool map has an overview
/// calibration (so <c>DemoTimeline.IsRadarCalibrated</c> is true), but Dust2 and Inferno only have crops registered
/// against the official radar, not yet checked on a real demo (<c>MapCalibration.RadarImageCrops</c> in Infrastructure), so until then
/// position-based features (2D replay, tactic matching) refuse them instead of drawing nonsense. World coordinates are kept
/// in every timeline, so adding a crop later only needs the radar fractions recomputed. Keep in sync with MapCalibration.</summary>
public static class MapRadarSupport
{
	#region Private Fields

	private static readonly HashSet<MapName> VerifiedMaps = [MapName.Ancient, MapName.Mirage, MapName.Cache, MapName.Anubis, MapName.Nuke];

	#endregion

	#region Public Methods

	/// <summary>Whether radar fractions on <paramref name="map"/> are verified to match the shipped radar image.</summary>
	public static bool HasVerifiedRadar(MapName? map) => map is { } m && VerifiedMaps.Contains(m);

	#endregion
}
