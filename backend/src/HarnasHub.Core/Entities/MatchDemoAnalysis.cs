namespace HarnasHub.Core.Entities;

/// <summary>Index row for a match's parsed demo timeline: the timeline itself lives as gzip JSON in object storage
/// under <see cref="ObjectKey"/>, this row only says it exists, which parser produced it and who "we" were in it.</summary>
public class MatchDemoAnalysis
{
	#region Public Properties

	/// <summary>Primary key.</summary>
	public Guid Id { get; set; }

	/// <summary>The match this timeline belongs to (unique — one timeline per match; loose link, like the rest of the app).</summary>
	public Guid MatchResultId { get; set; }

	/// <summary>Object storage key of the gzip JSON timeline.</summary>
	public string ObjectKey { get; set; } = string.Empty;

	/// <summary>Version of the demo parser that produced the timeline, so stale ones can be re-parsed or ignored.</summary>
	public int ParserVersion { get; set; }

	/// <summary>How many official rounds the timeline contains.</summary>
	public int RoundsCount { get; set; }

	/// <summary>SteamID64s of our team as they stood in round 1 — empty when it couldn't be determined.</summary>
	public List<long> OurTeamSteamIds { get; set; } = [];

	/// <summary>SteamID64s the coach/manager hid from this match's analysis (a coach, a stand-in...) — applied on read, the stored timeline is untouched.</summary>
	public List<long> ExcludedSteamIds { get; set; } = [];

	/// <summary>When the timeline was stored.</summary>
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
