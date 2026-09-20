namespace HarnasHub.Core.Entities;

/// <summary>One player's individual performance numbers for one <see cref="MatchResult"/>.</summary>
public class PlayerMatchStat
{
	#region Public Properties

	public Guid Id { get; set; }
	public Guid MatchResultId { get; set; }
	/// <summary>Null when this row came from a demo-imported player nobody on the roster has claimed with a matching
	/// SteamID64 yet — <see cref="DemoPlayerName"/> is the only identity available for it in that case.</summary>
	public Guid? UserId { get; set; }
	/// <summary>The demo's own name for this player — set only alongside a null <see cref="UserId"/>, so the row still
	/// displays as someone instead of silently vanishing until a coach connects it to a roster account.</summary>
	public string? DemoPlayerName { get; set; }
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
	/// <summary>This player's death locations for the match, JSON-serialized (radar-relative [0,1] fractions plus side) —
	/// only ever set on a row that came from a demo import, for the death-map view. Stored as a single JSON column rather
	/// than a child table since it's always read/written whole, never queried by individual point.</summary>
	public string? DeathPositionsJson { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
