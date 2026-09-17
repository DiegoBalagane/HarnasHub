namespace HarnasHub.Core.Entities;

/// <summary>One numbered spot on a tactic's radar; <see cref="X"/>/<see cref="Y"/> are radar-relative fractions in [0,1].</summary>
public class TacticPoint
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid TacticId { get; set; }
	public int Order { get; set; }
	public float X { get; set; }
	public float Y { get; set; }
	public string? Description { get; set; }
	public Guid? NadeEntryId { get; set; }

	#endregion
}
