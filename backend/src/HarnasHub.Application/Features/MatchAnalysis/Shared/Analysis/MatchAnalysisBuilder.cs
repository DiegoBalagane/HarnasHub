#region Usings

using HarnasHub.Application.Common.Maps;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;

/// <summary>Assembles the single-match deep analysis from the pure analyzers.</summary>
public static class MatchAnalysisBuilder
{
	#region Public Methods

	/// <summary>Runs every analyzer over <paramref name="context"/>; <paramref name="library"/> is the team's pinned nades on the match's map.</summary>
	public static MatchAnalysisDto Build(AnalysisContext context, IReadOnlyList<LibraryNade> library)
	{
		var map = context.Timeline.MapName;

		return new MatchAnalysisDto(
			map?.ToString(),
			map is { } m && MapZones.HasZones(m),
			context.Resolved,
			TradeAnalyzer.Analyze(context),
			ClutchAnalyzer.Analyze(context),
			OpeningDuelAnalyzer.Analyze(context),
			FlashAnalyzer.Analyze(context),
			GrenadeLibraryComparer.Compare(context, library));
	}

	#endregion
}
