using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>Where one player starts a round on a given map and side; <see cref="X"/>/<see cref="Y"/> are radar-relative fractions in [0,1].</summary>
public class MapPositionAssignment
{
	#region Public Properties

	public Guid Id { get; set; }
	public MapName MapName { get; set; }
	public MapSide Side { get; set; }
	public Guid UserId { get; set; }
	public string? Label { get; set; }
	public float X { get; set; }
	public float Y { get; set; }
	public string? Note { get; set; }
	public DateTime UpdatedAtUtc { get; set; }

	#endregion
}
