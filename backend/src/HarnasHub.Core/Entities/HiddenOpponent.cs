namespace HarnasHub.Core.Entities;

/// <summary>An opponent the team chose to hide from the list and name suggestions; its history (results, events, notes) stays untouched.</summary>
public class HiddenOpponent
{
	#region Public Properties

	/// <summary>Primary key.</summary>
	public Guid Id { get; set; }

	/// <summary>Normalized opponent name (<c>OpponentNames.ToKey</c>) — unique, one row per hidden opponent.</summary>
	public string OpponentKey { get; set; } = string.Empty;

	/// <summary>The opponent name as shown when it was hidden, so it can still be listed under "hidden" without any history.</summary>
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>When the opponent was hidden.</summary>
	public DateTime HiddenAtUtc { get; set; }

	/// <summary>Who hid the opponent.</summary>
	public Guid HiddenByUserId { get; set; }

	#endregion
}
