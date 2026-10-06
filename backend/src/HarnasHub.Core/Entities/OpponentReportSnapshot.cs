namespace HarnasHub.Core.Entities;

/// <summary>The last generated opponent report, stored so the whole team sees the same state and when its data is from.</summary>
public class OpponentReportSnapshot
{
	#region Public Properties

	public Guid Id { get; set; }
	/// <summary>Normalized opponent name — unique, the snapshot is replaced on every regeneration.</summary>
	public string OpponentKey { get; set; } = string.Empty;
	public DateTime GeneratedAtUtc { get; set; }
	/// <summary>When the underlying FACEIT data was last synced, null when the report was built without any sync.</summary>
	public DateTime? DataSyncedAtUtc { get; set; }
	/// <summary>The serialized report DTO.</summary>
	public string ReportJson { get; set; } = "{}";

	#endregion
}
