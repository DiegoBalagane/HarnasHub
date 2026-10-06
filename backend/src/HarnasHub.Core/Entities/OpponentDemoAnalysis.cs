#region Usings

using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Core.Entities;

/// <summary>Index row of one analysed opponent demo: the full timeline (with position samples) lives as gzip JSON in object
/// storage under <see cref="TimelineObjectKey"/>; this row says whose demo it is, which of its two teams is the opponent and
/// holds the small per-round facts the tendencies are aggregated from, so reading the report never touches storage.</summary>
public class OpponentDemoAnalysis
{
	#region Public Properties

	/// <summary>Primary key.</summary>
	public Guid Id { get; set; }

	/// <summary>Normalized opponent name (<c>OpponentNames.ToKey</c>).</summary>
	public string OpponentKey { get; set; } = string.Empty;

	/// <summary>The demo's map within the pool; null when it was played on a map outside it.</summary>
	public MapName? MapName { get; set; }

	/// <summary>Raw map name from the demo, e.g. "de_mirage".</summary>
	public string? RawMapName { get; set; }

	/// <summary>When the match was played, when known (FACEIT download); null for manual uploads.</summary>
	public DateTime? PlayedAtUtc { get; set; }

	/// <summary>Where the demo came from.</summary>
	public OpponentDemoSource Source { get; set; }

	/// <summary>FACEIT match id of a downloaded demo (with <see cref="FaceitMapNumber"/>), used to skip already analysed maps.</summary>
	public string? FaceitMatchId { get; set; }

	/// <summary>1-based map number within the FACEIT match.</summary>
	public int? FaceitMapNumber { get; set; }

	/// <summary>Object storage key of the gzip JSON timeline (<c>opponents/{key}/{id}.json.gz</c>).</summary>
	public string TimelineObjectKey { get; set; } = string.Empty;

	/// <summary>Version of the demo parser that produced the timeline.</summary>
	public int ParserVersion { get; set; }

	/// <summary>How many official rounds the timeline contains.</summary>
	public int RoundsCount { get; set; }

	/// <summary>Round-1 SteamID64s of the team that started T ("A").</summary>
	public List<long> TeamASteamIds { get; set; } = [];

	/// <summary>Round-1 SteamID64s of the team that started CT ("B").</summary>
	public List<long> TeamBSteamIds { get; set; } = [];

	/// <summary>Player names of team A, in the order of <see cref="TeamASteamIds"/>.</summary>
	public List<string> TeamANames { get; set; } = [];

	/// <summary>Player names of team B, in the order of <see cref="TeamBSteamIds"/>.</summary>
	public List<string> TeamBNames { get; set; } = [];

	/// <summary>SteamID64s of the opponent (round-1 roster of their team) — empty until detected or picked by the coach.</summary>
	public List<long> OpponentSteamIds { get; set; } = [];

	/// <summary>Per-round facts (<c>OpponentDemoFacts</c> JSON) for the tendencies; null until the opponent team is known.</summary>
	public string? FactsJson { get; set; }

	/// <summary>Who analysed the demo (null for background downloads).</summary>
	public Guid? CreatedByUserId { get; set; }

	/// <summary>When the analysis was stored.</summary>
	public DateTime CreatedAtUtc { get; set; }

	#endregion
}
