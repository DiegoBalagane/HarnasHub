#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Common.Maps;

/// <summary>Hand-estimated zone polygons per map, written in PIXELS of the radar images shipped in
/// <c>frontend/public/maps/*.webp</c> (so they can be checked/corrected by opening the image in any editor) and
/// converted to radar fractions here. APPROXIMATE: drawn by eye from the radar artwork, generous around the bombsite
/// markers and not verified against real player positions — verify against the radar images (and ideally a demo's
/// position cloud) before relying on exact callouts. Nuke has no A/B site polygons at all: its sites are stacked
/// vertically and can't be told apart from X/Y alone.</summary>
internal static class MapZoneData
{
	#region Public Properties

	/// <summary>All zones per map, ordered bombsites → mid → spawns → callouts so the most specific kind wins a lookup.</summary>
	public static IReadOnlyDictionary<MapName, IReadOnlyList<MapZone>> Zones { get; } = new Dictionary<MapName, IReadOnlyList<MapZone>>
	{
		[MapName.Mirage] = Map(1374, 1196,
			Rect("A", MapZoneKind.BombsiteA, 600, 840, 850, 1110),
			Rect("B", MapZoneKind.BombsiteB, 20, 115, 390, 410),
			Rect("Mid", MapZoneKind.Mid, 530, 430, 1050, 630),
			Rect("T Spawn", MapZoneKind.TSpawn, 1215, 250, 1360, 520),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 215, 820, 420, 1030),
			Rect("Top Mid", MapZoneKind.Callout, 930, 100, 1060, 430),
			Rect("Palace", MapZoneKind.Callout, 885, 940, 1230, 1095),
			Rect("A Ramp", MapZoneKind.Callout, 1060, 640, 1290, 880),
			Poly("Apartments", MapZoneKind.Callout, (105, 30), (1105, 30), (1105, 100), (840, 100), (840, 215), (490, 215), (490, 120), (105, 120))),

		[MapName.Ancient] = Map(1290, 1467,
			Rect("A", MapZoneKind.BombsiteA, 95, 210, 460, 450),
			Rect("B", MapZoneKind.BombsiteB, 895, 515, 1220, 730),
			Poly("Mid", MapZoneKind.Mid, (580, 440), (670, 440), (670, 650), (760, 650), (760, 900), (460, 900), (460, 650), (580, 650)),
			Rect("T Spawn", MapZoneKind.TSpawn, 550, 1140, 725, 1420),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 575, 45, 775, 295),
			Rect("Donut", MapZoneKind.Callout, 265, 600, 435, 740),
			Rect("A Main", MapZoneKind.Callout, 100, 850, 300, 960),
			Rect("Cave", MapZoneKind.Callout, 1085, 900, 1195, 1105)),

		[MapName.Anubis] = Map(2000, 2000,
			Rect("A", MapZoneKind.BombsiteA, 1290, 340, 1690, 700),
			Rect("B", MapZoneKind.BombsiteB, 400, 880, 800, 1130),
			Rect("Mid", MapZoneKind.Mid, 920, 860, 1160, 1250),
			Rect("T Spawn", MapZoneKind.TSpawn, 690, 1690, 1110, 1880),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 640, 300, 960, 515),
			Rect("Water", MapZoneKind.Callout, 1280, 860, 1480, 1180)),

		[MapName.Cache] = Map(1024, 1024,
			Rect("A", MapZoneKind.BombsiteA, 165, 165, 460, 360),
			Rect("B", MapZoneKind.BombsiteB, 285, 650, 445, 860),
			Rect("Mid", MapZoneKind.Mid, 450, 405, 690, 615),
			Rect("T Spawn", MapZoneKind.TSpawn, 840, 515, 970, 675),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 30, 385, 165, 640),
			Rect("A Main", MapZoneKind.Callout, 515, 215, 605, 405)),

		[MapName.Dust2] = Map(1516, 1619,
			Poly("A", MapZoneKind.BombsiteA, (930, 145), (1300, 145), (1300, 420), (1015, 420), (1015, 230), (930, 230)),
			Rect("B", MapZoneKind.BombsiteB, 30, 15, 355, 500),
			Rect("Mid", MapZoneKind.Mid, 630, 560, 790, 1000),
			Rect("T Spawn", MapZoneKind.TSpawn, 400, 1380, 720, 1530),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 840, 230, 1015, 420),
			Rect("Long A", MapZoneKind.Callout, 1270, 420, 1490, 1100),
			Rect("Upper Tunnels", MapZoneKind.Callout, 35, 600, 470, 800)),

		[MapName.Inferno] = Map(1500, 1491,
			Rect("A", MapZoneKind.BombsiteA, 1090, 890, 1310, 1160),
			Rect("B", MapZoneKind.BombsiteB, 595, 145, 815, 425),
			Rect("Mid", MapZoneKind.Mid, 700, 970, 1030, 1210),
			Rect("T Spawn", MapZoneKind.TSpawn, 30, 940, 140, 1140),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 1330, 400, 1440, 640),
			Rect("Banana", MapZoneKind.Callout, 570, 480, 760, 900)),

		[MapName.Nuke] = Map(1558, 848,
			Rect("T Spawn", MapZoneKind.TSpawn, 35, 370, 345, 560),
			Rect("CT Spawn", MapZoneKind.CTSpawn, 1100, 305, 1525, 490),
			Rect("Outside", MapZoneKind.Callout, 550, 600, 1010, 810),
			Rect("Lobby", MapZoneKind.Callout, 520, 300, 660, 560)),
	};

	#endregion

	#region Public Methods

	/// <summary>Whether demo radar fractions on this map line up with the shipped radar image. Dust2 and Inferno
	/// ship cropped images with no fitted crop yet (see <c>MapCalibration.RadarImageCrops</c>), so their positions are
	/// in plain overview space while these polygons are image-relative — their zones stay disabled until the crop is
	/// fitted against a real demo; then just add the map here.</summary>
	public static bool PositionsAlignWithRadar(MapName map) =>
		map is MapName.Mirage or MapName.Ancient or MapName.Anubis or MapName.Cache or MapName.Nuke;

	#endregion

	#region Private Methods

	private static IReadOnlyList<MapZone> Map(float width, float height, params Func<float, float, MapZone>[] zones) =>
		zones.Select(build => build(width, height)).OrderBy(z => z.Kind).ToList();

	private static Func<float, float, MapZone> Rect(string name, MapZoneKind kind, float left, float top, float right, float bottom) =>
		Poly(name, kind, (left, top), (right, top), (right, bottom), (left, bottom));

	private static Func<float, float, MapZone> Poly(string name, MapZoneKind kind, params (float X, float Y)[] pixels) =>
		(width, height) => new MapZone(name, kind, pixels.Select(p => (p.X / width, p.Y / height)).ToList());

	#endregion
}
