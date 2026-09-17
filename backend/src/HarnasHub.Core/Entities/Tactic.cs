using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A named, saved team playbook entry for one map and side, made up of any number of radar points.</summary>
public class Tactic
{
	#region Public Properties

	public Guid Id { get; set; }
	public MapName MapName { get; set; }
	public MapSide Side { get; set; }
	public string Name { get; set; } = string.Empty;
	public EconomyType Economy { get; set; }
	public string? Note { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }
	public List<TacticPoint> Points { get; set; } = [];

	#endregion
}
