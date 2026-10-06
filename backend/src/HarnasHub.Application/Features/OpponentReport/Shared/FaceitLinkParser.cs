using System.Text.RegularExpressions;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>What a coach pasted to link an opponent.</summary>
public enum FaceitLinkKind
{
	/// <summary>A FACEIT team page.</summary>
	Team = 0,
	/// <summary>A FACEIT match room.</summary>
	Match = 1,
	/// <summary>A list of FACEIT nicknames.</summary>
	Nicknames = 2
}

/// <summary>A parsed link source: <paramref name="Id"/> for team/match links, <paramref name="Nicknames"/> for a nickname list.</summary>
public record FaceitLinkSource(FaceitLinkKind Kind, string? Id, List<string> Nicknames);

/// <summary>Recognises a FACEIT team URL, a match room URL or a comma/space/newline separated list of nicknames.</summary>
public static partial class FaceitLinkParser
{
	#region Public Fields

	/// <summary>Most nicknames accepted in one list — a roster plus substitutes.</summary>
	public const int MaxNicknames = 10;

	#endregion

	#region Public Methods

	/// <summary>The parsed source, or null when the text is none of the accepted forms.</summary>
	public static FaceitLinkSource? Parse(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}

		var trimmed = text.Trim();
		if (trimmed.Contains("faceit.com", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("://", StringComparison.Ordinal))
		{
			var team = TeamUrl().Match(trimmed);
			if (team.Success)
			{
				return new FaceitLinkSource(FaceitLinkKind.Team, team.Groups["id"].Value.ToLowerInvariant(), []);
			}

			var room = RoomUrl().Match(trimmed);
			return room.Success ? new FaceitLinkSource(FaceitLinkKind.Match, room.Groups["id"].Value, []) : null;
		}

		var nicknames = Separators().Split(trimmed)
			.Where(n => n.Length > 0)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		return nicknames.Count is > 0 and <= MaxNicknames && nicknames.All(n => Nickname().IsMatch(n))
			? new FaceitLinkSource(FaceitLinkKind.Nicknames, null, nicknames)
			: null;
	}

	#endregion

	#region Private Methods

	/// <summary>e.g. https://www.faceit.com/en/teams/0b1c…-uuid.</summary>
	[GeneratedRegex(@"/teams/(?<id>[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})", RegexOptions.IgnoreCase)]
	private static partial Regex TeamUrl();

	/// <summary>e.g. https://www.faceit.com/en/cs2/room/1-0b1c…-uuid (optionally followed by /scoreboard).</summary>
	[GeneratedRegex(@"/room/(?<id>(?:\d+-)?[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})", RegexOptions.IgnoreCase)]
	private static partial Regex RoomUrl();

	/// <summary>Commas, semicolons and any whitespace.</summary>
	[GeneratedRegex(@"[\s,;]+")]
	private static partial Regex Separators();

	/// <summary>FACEIT nicknames: letters, digits, "-" and "_".</summary>
	[GeneratedRegex(@"^[A-Za-z0-9_\-]{2,32}$")]
	private static partial Regex Nickname();

	#endregion
}
