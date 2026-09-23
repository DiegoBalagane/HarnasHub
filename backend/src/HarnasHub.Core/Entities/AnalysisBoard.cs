using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A freehand drawing saved over either the built-in radar for <see cref="MapName"/> or a coach-uploaded
/// screenshot — for sketching out a setup/analysis and coming back to edit it later. Grouped by map in the UI.</summary>
public class AnalysisBoard
{
	#region Public Properties

	public Guid Id { get; set; }
	public MapName MapName { get; set; }
	public string Title { get; set; } = string.Empty;
	/// <summary>S3 object key for a coach-uploaded background screenshot; null draws over the built-in radar image instead.</summary>
	public string? BackgroundImageObjectKey { get; set; }
	/// <summary>Freehand strokes as JSON (color/width/points, radar-relative [0,1] fractions like every other pin on this
	/// app's maps) — replayed onto the canvas on load, same reasoning as <c>PlayerMatchStat.DeathPositionsJson</c> for
	/// storing it as one column instead of a child table: always read/written whole.</summary>
	public string StrokesJson { get; set; } = "[]";
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public DateTime UpdatedAtUtc { get; set; }

	#endregion
}
