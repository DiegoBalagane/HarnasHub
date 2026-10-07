#region Usings

using System.Text.RegularExpressions;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Reads the ESEA League season number from a FACEIT competition name. FACEIT names ESEA divisions like
/// "S59 EU Open10 D - Regular Season" or "S58 EU Intermediate B - Playoffs"; older/other spellings say "ESEA … Season 59" or
/// "ESEA S59 …". Cups that merely contain "S01" ("Eagle E-Sports | Classic Series | S01 T08") are not ESEA.</summary>
public static partial class EseaSeasonParser
{
	#region Public Methods

	/// <summary>The ESEA season number of <paramref name="competitionName"/>, or null when it isn't an ESEA League competition.</summary>
	public static int? Parse(string? competitionName)
	{
		if (string.IsNullOrWhiteSpace(competitionName))
		{
			return null;
		}

		var match = DivisionName().Match(competitionName);
		if (!match.Success)
		{
			match = EseaPrefixed().Match(competitionName);
		}

		if (!match.Success)
		{
			match = SeasonThenEsea().Match(competitionName);
		}

		return match.Success && int.TryParse(match.Groups["season"].Value, out var season) && season > 0 ? season : null;
	}

	/// <summary>Short label of a season ("S59").</summary>
	public static string Label(int season) => $"S{season}";

	#endregion

	#region Private Methods

	/// <summary>"S59 EU Open10 D - Regular Season": the name starts with the season and a region code.</summary>
	[GeneratedRegex(@"^\s*S(?<season>\d{1,3})\s+(?:EU|NA|SA|OCE|AS|ASIA|ME|MENA|SEA|CIS|AF)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex DivisionName();

	/// <summary>"ESEA S59 …", "ESEA League Season 59 …", "ESEA Open Season 50 …".</summary>
	[GeneratedRegex(@"\bESEA\b.*?\b(?:S|Season\s*)(?<season>\d{1,3})\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex EseaPrefixed();

	/// <summary>"Season 59 ESEA …".</summary>
	[GeneratedRegex(@"\bSeason\s*(?<season>\d{1,3})\b.*\bESEA\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex SeasonThenEsea();

	#endregion
}
