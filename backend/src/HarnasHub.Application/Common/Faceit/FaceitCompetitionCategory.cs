#region Usings

using System.Text.RegularExpressions;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Common.Faceit;

/// <summary>Conservative guess of a result's <see cref="MatchCategory"/> from a FACEIT competition: only championships are
/// promoted (to League when the name says so, e.g. ESEA, otherwise Tournament); matchmaking, hubs and anything unknown stay
/// Scrimmage. Always just a prefill the coach can change.</summary>
public static partial class FaceitCompetitionCategory
{
	#region Private Fields

	private static readonly string[] LeagueMarkers = ["league", "liga", "esea"];

	#endregion

	#region Public Methods

	/// <summary>The category to prefill for a competition of <paramref name="competitionType"/> named <paramref name="competitionName"/>.</summary>
	public static MatchCategory Map(string? competitionType, string? competitionName)
	{
		if (!string.Equals(competitionType?.Trim(), "championship", StringComparison.OrdinalIgnoreCase))
		{
			return MatchCategory.Scrimmage;
		}

		var name = competitionName ?? string.Empty;
		// ESEA season divisions are named like "S59 EU Open10 D - Regular Season" — no "league" in the name, still a league.
		return LeagueMarkers.Any(marker => name.Contains(marker, StringComparison.OrdinalIgnoreCase)) || EseaSeasonPrefix().IsMatch(name)
			? MatchCategory.League
			: MatchCategory.Tournament;
	}

	#endregion

	#region Private Methods

	[GeneratedRegex(@"^\s*S\d{1,3}\s", RegexOptions.IgnoreCase)]
	private static partial Regex EseaSeasonPrefix();

	#endregion
}
