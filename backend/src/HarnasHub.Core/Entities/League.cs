using HarnasHub.Core.Enums;

namespace HarnasHub.Core.Entities;

/// <summary>A named league season/division that groups several <see cref="MatchResult"/> rows together.</summary>
public class League
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Name { get; set; } = string.Empty;
	/// <summary>Free-text season label, e.g. "2026 Wiosna".</summary>
	public string Season { get; set; } = string.Empty;
	public LeagueType Type { get; set; }
	public Guid CreatedByUserId { get; set; }
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
