#region Usings

using HarnasHub.Application.Common.Notifications;
using HarnasHub.Application.Features.OpponentReport.Tendencies;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Pure Polish digest for the opponent scouting channel after opponent demos were analysed: the top anti-strat suggestions.</summary>
public static class OpponentDigestFormatter
{
	#region Public Fields

	/// <summary>Most suggestions listed in the digest, across all maps.</summary>
	public const int MaxSuggestions = 5;

	#endregion

	#region Public Methods

	/// <summary>Header with the new demos' maps, up to <see cref="MaxSuggestions"/> suggestions (higher confidence first) grouped by map, and a link when <paramref name="reportUrl"/> is set.</summary>
	public static string Format(string opponent, IReadOnlyCollection<string> newDemoMaps, IReadOnlyList<MapTendenciesDto> tendencies, string? reportUrl)
	{
		var maps = newDemoMaps.Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList();
		var header = maps.Count == 0
			? $"🕵️ **Nowa demka przeciwnika: {opponent}**"
			: $"🕵️ **Nowa demka przeciwnika: {opponent}** ({string.Join(", ", maps)})";
		var lines = new List<string> { header };

		var remaining = MaxSuggestions;
		foreach (var map in tendencies.Where(t => t.Suggestions.Count > 0))
		{
			if (remaining == 0)
			{
				break;
			}

			var picked = map.Suggestions.OrderBy(s => ConfidenceRank(s.Confidence)).Take(remaining).ToList();
			lines.Add($"**{map.MapName}** (z {map.Demos} demek):");
			lines.AddRange(picked.Select(s => $"• {s.Text}"));
			remaining -= picked.Count;
		}

		if (remaining == MaxSuggestions)
		{
			lines.Add("Za mało danych na wnioski anti-strat.");
		}

		var link = string.IsNullOrWhiteSpace(reportUrl) ? null : $"\n🔗 Raport przeciwnika: {reportUrl}";
		return DiscordMessage.WithFooter(string.Join('\n', lines), link);
	}

	#endregion

	#region Private Methods

	private static int ConfidenceRank(string confidence) => confidence switch
	{
		"High" => 0,
		"Medium" => 1,
		_ => 2
	};

	#endregion
}
