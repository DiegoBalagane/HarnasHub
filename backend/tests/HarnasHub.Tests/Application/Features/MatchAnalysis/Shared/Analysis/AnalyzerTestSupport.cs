#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Tests.Application.Features.Tactics;

#endregion

namespace HarnasHub.Tests.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Shared builders for the analyzer tests: team A (SteamIDs 1, 2) is ours and starts on T.</summary>
internal static class AnalyzerTestSupport
{
	#region Internal Methods

	/// <summary>A context where we are team A, over a timeline of the given rounds/kills/blinds/grenades.</summary>
	internal static AnalysisContext Context(
		IReadOnlyList<DemoTimelineRound> rounds,
		IReadOnlyList<DemoKill>? kills = null,
		IReadOnlyList<DemoBlind>? blinds = null,
		IReadOnlyList<DemoGrenade>? grenades = null)
	{
		var timeline = DemoTimelineFactory.Timeline(rounds, grenades ?? []) with { Kills = kills ?? [], Blinds = blinds ?? [] };
		return AnalysisContext.Create(timeline, DemoTimelineFactory.TeamA);
	}

	#endregion
}
