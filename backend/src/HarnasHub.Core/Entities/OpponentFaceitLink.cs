namespace HarnasHub.Core.Entities;

/// <summary>Ties an opponent (by its normalized name key) to the FACEIT players — and optionally the FACEIT team — that make it up.</summary>
public class OpponentFaceitLink
{
	#region Public Properties

	public Guid Id { get; set; }
	/// <summary>Normalized opponent name (<c>OpponentNames.ToKey</c>) — unique, one link per opponent.</summary>
	public string OpponentKey { get; set; } = string.Empty;
	/// <summary>The opponent name as typed by the coach when linking.</summary>
	public string DisplayName { get; set; } = string.Empty;
	/// <summary>FACEIT team id when the link was made from a team page.</summary>
	public string? FaceitTeamId { get; set; }
	/// <summary>FACEIT player ids treated as the opponent's roster.</summary>
	public List<string> PlayerIds { get; set; } = [];
	public Guid LinkedByUserId { get; set; }
	public DateTime LinkedAtUtc { get; set; }
	/// <summary>Last successful sync of the roster's match history; drives the manual refresh cooldown.</summary>
	public DateTime? LastSyncedAtUtc { get; set; }

	#endregion
}
