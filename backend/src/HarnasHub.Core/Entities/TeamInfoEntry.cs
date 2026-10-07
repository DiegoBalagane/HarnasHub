namespace HarnasHub.Core.Entities;

/// <summary>A constant technical fact of the team (Discord invite, server address, config snippet) shown on the Info page.</summary>
public class TeamInfoEntry
{
	#region Public Properties

	public Guid Id { get; set; }
	public string Category { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public string Value { get; set; } = string.Empty;

	/// <summary>When true the UI masks the value until a team member explicitly reveals it (e.g. server or RCON passwords).</summary>
	public bool IsSecret { get; set; }

	public int SortOrder { get; set; }
	public DateTime UpdatedAtUtc { get; set; }
	public Guid UpdatedByUserId { get; set; }

	#endregion
}
