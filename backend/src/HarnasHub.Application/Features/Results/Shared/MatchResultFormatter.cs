#region Usings

using HarnasHub.Application.Common.Notifications;

#endregion

namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>Pure Polish text of the "result saved" notification for the match schedule channel.</summary>
public static class MatchResultFormatter
{
	#region Public Methods

	/// <summary>Opponent, score, verdict, map and a link line when <paramref name="resultUrl"/> is set.</summary>
	public static string Format(string opponent, int ourScore, int opponentScore, string? mapName, string? resultUrl)
	{
		var (icon, verdict) = ourScore > opponentScore ? ("🏆", "wygrana") : ourScore < opponentScore ? ("💔", "porażka") : ("🤝", "remis");
		var map = string.IsNullOrWhiteSpace(mapName) ? "" : $" — {mapName.Trim()}";
		var link = string.IsNullOrWhiteSpace(resultUrl) ? null : $"\n🔗 Szczegóły meczu: {resultUrl}";

		return DiscordMessage.WithFooter($"{icon} Wynik meczu vs **{opponent}**: **{ourScore}:{opponentScore}** ({verdict}){map}", link);
	}

	#endregion
}
