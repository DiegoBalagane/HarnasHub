using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A free-floating text label on a map's radar (e.g. a callout note), not tied to any player —
/// unlike <see cref="MapPositionAssignment"/>, many can exist on the same map/side. <see cref="X"/>/<see cref="Y"/>
/// are radar-relative fractions in [0,1], same convention as <c>MapPositionAssignment.X/Y</c>.</summary>
public class MapTextAnnotation
{
	#region Public Properties

	public Guid Id { get; set; }
	public MapName MapName { get; set; }
	public MapSide Side { get; set; }
	public string Text { get; set; } = string.Empty;
	/// <summary>Hex color, e.g. "#ffffff".</summary>
	public string Color { get; set; } = "#ffffff";
	public int FontSizePx { get; set; } = 14;
	public float X { get; set; }
	public float Y { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime UpdatedAtUtc { get; set; }

	#endregion
}
