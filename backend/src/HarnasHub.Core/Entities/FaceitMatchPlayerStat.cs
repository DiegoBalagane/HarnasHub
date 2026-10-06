namespace HarnasHub.Core.Entities;

/// <summary>One player's scoreboard line on one map of a cached <see cref="FaceitMatch"/>.</summary>
public class FaceitMatchPlayerStat
{
	#region Public Properties

	public Guid Id { get; set; }
	/// <summary>The <see cref="FaceitMatch.Id"/> row (one map) this line belongs to.</summary>
	public Guid MatchId { get; set; }
	public string PlayerId { get; set; } = string.Empty;
	public string Nickname { get; set; } = string.Empty;
	/// <summary>1 or 2 — which team of the match the player was on.</summary>
	public int Team { get; set; }
	public int Kills { get; set; }
	public int Deaths { get; set; }
	public int Assists { get; set; }
	/// <summary>Average damage per round, when FACEIT reports it.</summary>
	public double? Adr { get; set; }
	public double? HeadshotPercent { get; set; }
	public int TripleKills { get; set; }
	public int QuadroKills { get; set; }
	public int PentaKills { get; set; }
	public int Mvps { get; set; }

	#endregion
}
