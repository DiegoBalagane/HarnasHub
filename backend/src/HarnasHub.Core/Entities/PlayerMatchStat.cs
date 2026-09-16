namespace HarnasHub.Core.Entities;

/// <summary>One player's individual performance numbers for one <see cref="MatchResult"/>.</summary>
public class PlayerMatchStat
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid MatchResultId { get; set; }
	public Guid UserId { get; set; }
	public int Kills { get; set; }
	public int Deaths { get; set; }
	public int Assists { get; set; }
	public double Adr { get; set; }
	public double HeadshotPercentage { get; set; }
	public double Rating { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
