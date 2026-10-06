#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Demos;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>A timeline together with who "we" are in it, shared by every pure analyzer. When the team is unresolved
/// "we" fall back to the team that started on T (same convention as <see cref="MatchTimelineMapper"/>).</summary>
public sealed class AnalysisContext
{
	#region Private Fields

	private readonly HashSet<long> _ours;
	private readonly Dictionary<long, string> _names;

	#endregion

	#region Constructors

	private AnalysisContext(DemoTimeline timeline, HashSet<long> ours, bool resolved)
	{
		Timeline = timeline;
		_ours = ours;
		Resolved = resolved;
		_names = TimelinePlayerNames.Collect(timeline);
	}

	#endregion

	#region Public Properties

	/// <summary>The parsed timeline.</summary>
	public DemoTimeline Timeline { get; }

	/// <summary>False when "us" could not be determined and the fallback team is used.</summary>
	public bool Resolved { get; }

	#endregion

	#region Public Methods

	/// <summary>Creates the context for <paramref name="ourTeamSteamIds"/> (empty = unresolved, fall back to the round-1 T team).</summary>
	public static AnalysisContext Create(DemoTimeline timeline, IReadOnlyCollection<long> ourTeamSteamIds)
	{
		var resolved = ourTeamSteamIds.Count > 0;
		var ours = resolved
			? ourTeamSteamIds.ToHashSet()
			: (timeline.Rounds.OrderBy(r => r.Number).FirstOrDefault()?.TerroristSteamIds ?? []).ToHashSet();

		return new AnalysisContext(timeline, ours, resolved);
	}

	/// <summary>Whether the player is on our team.</summary>
	public bool IsOurs(long steamId64) => _ours.Contains(steamId64);

	/// <summary>The side our team played in <paramref name="round"/>, or null when none of us took part.</summary>
	public MapSide? OurSide(DemoTimelineRound round) => TimelineTeamResolver.OurSide(round, _ours);

	/// <summary>The player's display name (falls back to the SteamID64 text).</summary>
	public string Name(long steamId64) => _names.TryGetValue(steamId64, out var name) ? name : steamId64.ToString();

	#endregion
}
