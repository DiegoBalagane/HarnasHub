namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Recent form of a roster: last team games, current streak and lineup changes.</summary>
public static class TeamFormCalculator
{
	#region Public Fields

	/// <summary>How many latest team games are listed.</summary>
	public const int LastGamesCount = 10;

	/// <summary>Players first seen in this many latest games count as new.</summary>
	public const int RecentWindow = 5;

	/// <summary>Lineup changes are only reported when at least this many older games exist to compare against.</summary>
	public const int MinEarlierGames = 3;

	#endregion

	#region Public Methods

	/// <summary>Builds the form from team games (any order) and a player id → nickname map for naming new players; <paramref name="ignored"/> (linked ex-members/subs outside the active lineup) never count as new.</summary>
	public static OpponentFormDto Calculate(IEnumerable<TeamGame> games, IReadOnlyDictionary<string, string> nicknames, IReadOnlySet<string>? ignored = null)
	{
		var ordered = games.OrderByDescending(g => g.PlayedAtUtc).ToList();

		var lastGames = ordered
			.Take(LastGamesCount)
			.Select(g => new FormGameDto(
				g.FaceitMatchId,
				g.PlayedAtUtc,
				g.Map?.ToString() ?? g.RawMapName,
				g.RoundsFor,
				g.RoundsAgainst,
				g.Won,
				g.CompetitionName))
			.ToList();

		return new OpponentFormDto(lastGames, Streak(ordered), NewPlayers(ordered, nicknames, ignored));
	}

	/// <summary>"W3" for three wins in a row, "L2" for two losses; null without games.</summary>
	public static string? Streak(IReadOnlyList<TeamGame> newestFirst)
	{
		if (newestFirst.Count == 0)
		{
			return null;
		}

		var won = newestFirst[0].Won;
		var length = newestFirst.TakeWhile(g => g.Won == won).Count();
		return $"{(won ? "W" : "L")}{length}";
	}

	#endregion

	#region Private Methods

	/// <summary>Nicknames of players on their side in the latest games who never appeared in the older ones.</summary>
	private static List<string> NewPlayers(List<TeamGame> newestFirst, IReadOnlyDictionary<string, string> nicknames, IReadOnlySet<string>? ignored)
	{
		var earlier = newestFirst.Skip(RecentWindow).ToList();
		if (earlier.Count < MinEarlierGames)
		{
			return [];
		}

		var known = earlier.SelectMany(g => g.SidePlayerIds).ToHashSet();
		var newIds = newestFirst
			.Take(RecentWindow)
			.SelectMany(g => g.SidePlayerIds)
			.Where(id => !known.Contains(id) && ignored?.Contains(id) != true)
			.Distinct()
			.ToList();

		// Only linked players are cached with a nickname; a stand-in outside the list would otherwise surface as a raw
		// FACEIT id, so those are summarised as a count instead.
		var names = newIds
			.Where(nicknames.ContainsKey)
			.Select(id => nicknames[id])
			.OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
			.ToList();
		var outsiders = newIds.Count - names.Count;
		if (outsiders > 0)
		{
			names.Add(outsiders == 1 ? "1 gracz spoza listy" : $"{outsiders} graczy spoza listy");
		}

		return names;
	}

	#endregion
}
