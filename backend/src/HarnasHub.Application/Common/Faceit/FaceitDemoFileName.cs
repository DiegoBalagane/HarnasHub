#region Usings

using System.Text.RegularExpressions;

#endregion

namespace HarnasHub.Application.Common.Faceit;

/// <summary>A FACEIT match recognised from a demo file name; <paramref name="MapNumber"/> is the 1-based map of a series
/// when the name carries one.</summary>
public sealed record FaceitDemoFileReference(string MatchId, int? MapNumber);

/// <summary>Recognises FACEIT demo file names such as <c>1-8da435dc-78fc-42f1-85ca-02f3e4d02a7e-1-2.dem</c>: the match id
/// (<c>1-&lt;uuid&gt;</c>) followed by up to two small numbers, the last of which is taken as the map number. Anything
/// else (renamed files, other platforms) is ignored.</summary>
public static partial class FaceitDemoFileName
{
	#region Public Methods

	/// <summary>The FACEIT match a demo file name points at, or null when the name isn't a FACEIT demo name.</summary>
	public static FaceitDemoFileReference? Parse(string? fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			return null;
		}

		// Browsers send just the name, but some report "C:\fakepath\name.dem" — keep only the last segment either way.
		var name = fileName.Trim();
		var lastSeparator = name.LastIndexOfAny(['/', '\\']);
		if (lastSeparator >= 0)
		{
			name = name[(lastSeparator + 1)..];
		}

		var match = DemoName().Match(name);
		if (!match.Success)
		{
			return null;
		}

		var numberGroup = match.Groups["second"].Success ? match.Groups["second"] : match.Groups["first"];
		int? mapNumber = numberGroup.Success && int.TryParse(numberGroup.Value, out var number) && number is >= 1 and <= 9 ? number : null;
		return new FaceitDemoFileReference(match.Groups["id"].Value.ToLowerInvariant(), mapNumber);
	}

	#endregion

	#region Private Methods

	[GeneratedRegex(
		@"^(?<id>1-[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})(?:-(?<first>\d{1,2}))?(?:-(?<second>\d{1,2}))?(?: \(\d+\))?(?:\.dem)?(?:\.(?:gz|zst|bz2))?$",
		RegexOptions.IgnoreCase)]
	private static partial Regex DemoName();

	#endregion
}
