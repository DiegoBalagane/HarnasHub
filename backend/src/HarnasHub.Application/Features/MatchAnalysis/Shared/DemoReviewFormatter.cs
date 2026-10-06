#region Usings

using HarnasHub.Application.Common.Notifications;
using HarnasHub.Core.Enums;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.Shared;

/// <summary>Pure Polish digest posted to the demo review channel after a demo is attached to a result.</summary>
public static class DemoReviewFormatter
{
	#region Public Fields

	/// <summary>Most insights listed in the digest.</summary>
	public const int MaxInsights = 5;

	#endregion

	#region Public Methods

	/// <summary>Opponent, score, map, the top insights (problems first) and a link when <paramref name="matchUrl"/> is set.</summary>
	public static string Format(
		string opponent, int ourScore, int opponentScore, string? mapName, IReadOnlyList<MatchInsightDto> insights, string? matchUrl)
	{
		var map = string.IsNullOrWhiteSpace(mapName) ? "" : $" ({mapName.Trim()})";
		var lines = new List<string> { $"🎬 **Analiza demki: vs {opponent}** {ourScore}:{opponentScore}{map}" };

		var top = insights.Where(i => i.Code != "team-unknown").Take(MaxInsights).ToList();
		if (top.Count == 0)
		{
			lines.Add("Demka dołączona do meczu — za mało danych na wnioski.");
		}
		else
		{
			lines.Add("**Najważniejsze wnioski:**");
			lines.AddRange(top.Select(i => $"{ToneIcon(i.Tone)} **{i.Title}** — {i.Detail}"));
		}

		var link = string.IsNullOrWhiteSpace(matchUrl) ? null : $"\n🔗 Pełna analiza: {matchUrl}";
		return DiscordMessage.WithFooter(string.Join('\n', lines), link);
	}

	#endregion

	#region Private Methods

	private static string ToneIcon(InsightTone tone) => tone switch
	{
		InsightTone.Negative => "🔴",
		InsightTone.Positive => "🟢",
		_ => "⚪"
	};

	#endregion
}
