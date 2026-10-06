using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>The team's stance on one map of the pool; a map with no entry simply hasn't been classified yet.</summary>
public class MapPoolEntry
{
	public Guid Id { get; set; }
	/// <summary>The map this entry classifies — unique, one entry per map.</summary>
	public MapName MapName { get; set; }
	public MapPoolStatus Status { get; set; }
	/// <summary>Optional short coach remark, e.g. "CT side still weak".</summary>
	public string? Note { get; set; }
	public Guid UpdatedByUserId { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
}
