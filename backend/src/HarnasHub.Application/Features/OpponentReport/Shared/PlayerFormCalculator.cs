namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Per-player individual form from <see cref="IndividualGameLine"/>s: totals, per-map numbers split team vs solo, and the
/// recent-form arrow.</summary>
public static class PlayerFormCalculator
{
	#region Public Fields

	/// <summary>How many latest games form the "recent" side of the form arrow.</summary>
	public const int RecentGames = 10;

	/// <summary>The arrow is only drawn when at least this many older games exist to compare against.</summary>
	public const int MinEarlierGames = 5;

	/// <summary>A K/D change of at least this much (either way) turns the arrow up or down; smaller changes are "Flat".</summary>
	public const double KdThreshold = 0.15;

	#endregion

	#region Public Methods

	/// <summary>One entry per player in <paramref name="players"/> (in that order, players without games included) plus any other
	/// player found in <paramref name="lines"/>; the latest scoreboard nickname wins over the cached one.</summary>
	public static List<PlayerFormDto> Calculate(IEnumerable<IndividualGameLine> lines, IReadOnlyList<FaceitPlayerDto> players)
	{
		var byPlayer = lines.GroupBy(l => l.PlayerId).ToDictionary(g => g.Key, g => g.OrderBy(l => l.PlayedAtUtc).ToList());
		var ids = players.Select(p => p.PlayerId).Concat(byPlayer.Keys).Distinct().ToList();
		var profiles = players.GroupBy(p => p.PlayerId).ToDictionary(g => g.Key, g => g.First());

		return ids
			.Select(id => ToPlayer(id, profiles.GetValueOrDefault(id), byPlayer.GetValueOrDefault(id) ?? []))
			.OrderByDescending(p => p.Games)
			.ThenBy(p => p.Nickname, StringComparer.OrdinalIgnoreCase)
			.ToList();
	}

	/// <summary>Last <see cref="RecentGames"/> games (newest first) vs the earlier ones; null without enough games on both sides.</summary>
	public static PlayerRecentFormDto? RecentForm(IReadOnlyList<IndividualGameLine> newestFirst)
	{
		var recent = newestFirst.Take(RecentGames).ToList();
		var earlier = newestFirst.Skip(RecentGames).ToList();
		if (recent.Count < RecentGames || earlier.Count < MinEarlierGames)
		{
			return null;
		}

		var recentKd = Kd(recent);
		var earlierKd = Kd(earlier);
		var recentWr = WinRate(recent);
		var earlierWr = WinRate(earlier);
		var kdDelta = Math.Round(recentKd - earlierKd, 2);
		var direction = kdDelta >= KdThreshold ? "Up" : kdDelta <= -KdThreshold ? "Down" : "Flat";

		return new PlayerRecentFormDto(
			recent.Count,
			earlier.Count,
			recentKd,
			earlierKd,
			recentWr,
			earlierWr,
			kdDelta,
			Math.Round(recentWr - earlierWr, 1),
			direction);
	}

	/// <summary>Kills over deaths (kills alone without deaths), two decimals.</summary>
	public static double Kd(IReadOnlyCollection<IndividualGameLine> lines)
	{
		var kills = lines.Sum(l => l.Kills);
		var deaths = lines.Sum(l => l.Deaths);
		return Math.Round(deaths == 0 ? kills : (double)kills / deaths, 2);
	}

	/// <summary>Win rate in percent with one decimal; 0 for an empty list.</summary>
	public static double WinRate(IReadOnlyCollection<IndividualGameLine> lines) =>
		lines.Count == 0 ? 0 : Math.Round(100.0 * lines.Count(l => l.Won) / lines.Count, 1);

	#endregion

	#region Private Methods

	/// <summary>Builds one player's entry from their lines (oldest first).</summary>
	private static PlayerFormDto ToPlayer(string id, FaceitPlayerDto? profile, List<IndividualGameLine> lines)
	{
		var nickname = lines.Count > 0 ? lines[^1].Nickname : profile?.Nickname ?? id;
		var maps = lines
			.Where(l => l.Map.HasValue)
			.GroupBy(l => l.Map!.Value)
			.Select(g => ToMap(g.ToList(), lines.Count))
			.OrderByDescending(m => m.Games)
			.ThenBy(m => m.MapName)
			.ToList();

		return new PlayerFormDto(
			id,
			nickname,
			profile?.Elo,
			profile?.SkillLevel,
			lines.Count,
			lines.Count(l => l.TeamGame),
			lines.Count(l => !l.TeamGame),
			lines.Count == 0 ? null : WinRate(lines),
			lines.Count == 0 ? null : Kd(lines),
			Average(lines, l => l.Adr),
			Average(lines, l => l.HeadshotPercent),
			lines.Count == 0 ? null : lines[^1].PlayedAtUtc,
			RecentForm(Enumerable.Reverse(lines).ToList()),
			maps);
	}

	/// <summary>One player's numbers on one map; the share is relative to all their games in the window.</summary>
	private static PlayerMapFormDto ToMap(List<IndividualGameLine> onMap, int totalGames)
	{
		var team = onMap.Where(l => l.TeamGame).ToList();
		var solo = onMap.Where(l => !l.TeamGame).ToList();

		return new PlayerMapFormDto(
			onMap[0].Map!.Value.ToString(),
			onMap.Count,
			Math.Round(100.0 * onMap.Count / totalGames, 1),
			onMap.Count(l => l.Won),
			WinRate(onMap),
			Kd(onMap),
			Average(onMap, l => l.Adr),
			Average(onMap, l => l.HeadshotPercent),
			onMap.Max(l => l.PlayedAtUtc),
			team.Count,
			solo.Count,
			team.Count == 0 ? null : WinRate(team),
			solo.Count == 0 ? null : WinRate(solo),
			team.Count == 0 ? null : Kd(team),
			solo.Count == 0 ? null : Kd(solo));
	}

	/// <summary>Average of the known values with one decimal; null when none is known.</summary>
	private static double? Average(IEnumerable<IndividualGameLine> lines, Func<IndividualGameLine, double?> selector)
	{
		var values = lines.Select(selector).Where(v => v.HasValue).Select(v => v!.Value).ToList();
		return values.Count == 0 ? null : Math.Round(values.Average(), 1);
	}

	#endregion
}
