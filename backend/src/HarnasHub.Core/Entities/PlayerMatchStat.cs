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
	/// <summary>Everything below is only ever set when this row came from a demo import (see <c>ImportStatsFromDemo</c>) — null on a manually entered row, since there's no demo to compute it from.</summary>
	public int? EntryKills { get; set; }
	public int? EntryDeaths { get; set; }
	/// <summary>% of rounds this player got a Kill/Assist, Survived, or was Traded — a more honest "involvement" measure than raw K/D.</summary>
	public double? KastPercentage { get; set; }
	public int? MultiKill2K { get; set; }
	public int? MultiKill3K { get; set; }
	public int? MultiKill4K { get; set; }
	public int? MultiKill5K { get; set; }
	public int? UtilityDamage { get; set; }
	public int? FlashAssists { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
