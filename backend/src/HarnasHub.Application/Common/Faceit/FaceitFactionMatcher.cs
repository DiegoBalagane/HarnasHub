#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Application.Common.Faceit;

/// <summary>Pure rules telling the two factions of a FACEIT match room apart: which one is us, which one is a given
/// opponent, and which demo team ("A" started on T, "B" on CT) each faction played as.</summary>
public static class FaceitFactionMatcher
{
	#region Public Methods

	/// <summary>Index of the only faction containing one of our players (by SteamID64 or cached FACEIT id); null when none or both do.</summary>
	public static int? OurFactionIndex(
		IReadOnlyList<FaceitFactionInfo> factions, IReadOnlySet<string> ourSteamIds, IReadOnlySet<string> ourFaceitIds)
	{
		var withUs = factions
			.Select((faction, index) => (faction, index))
			.Where(f => f.faction.Players.Any(p =>
				ourFaceitIds.Contains(p.PlayerId) || (p.SteamId64 is not null && ourSteamIds.Contains(p.SteamId64.Trim()))))
			.Select(f => f.index)
			.ToList();
		return withUs.Count == 1 ? withUs[0] : null;
	}

	/// <summary>Index of the faction holding the most of <paramref name="playerIds"/>; null on no overlap or a tie.</summary>
	public static int? FactionWithMostPlayers(IReadOnlyList<FaceitFactionInfo> factions, IReadOnlySet<string> playerIds)
	{
		if (playerIds.Count == 0 || factions.Count == 0)
		{
			return null;
		}

		var counts = factions.Select(f => f.Players.Count(p => playerIds.Contains(p.PlayerId))).ToList();
		var best = counts.Max();
		return best > 0 && counts.Count(c => c == best) == 1 ? counts.IndexOf(best) : null;
	}

	/// <summary>The opponent's faction: the one with their known FACEIT players, else the one that isn't us, else the only
	/// one named like them ("team_Nick" rooms rarely are); null when nothing decides it.</summary>
	public static int? OpponentFactionIndex(
		IReadOnlyList<FaceitFactionInfo> factions, int? ourFactionIndex, IReadOnlySet<string> knownOpponentPlayerIds, string opponentKey)
	{
		if (factions.Count != 2)
		{
			return null;
		}

		if (FactionWithMostPlayers(factions, knownOpponentPlayerIds) is { } known)
		{
			return known;
		}

		if (ourFactionIndex is { } ours)
		{
			return 1 - ours;
		}

		var named = factions
			.Select((faction, index) => (faction, index))
			.Where(f => NameMatches(f.faction.Name, opponentKey))
			.Select(f => f.index)
			.ToList();
		return named.Count == 1 ? named[0] : null;
	}

	/// <summary>Demo team ("A"/"B") the faction played as, by SteamID64 overlap with the round-1 rosters; null when undecided.</summary>
	public static string? DemoTeam(FaceitFactionInfo faction, IReadOnlyCollection<long> teamA, IReadOnlyCollection<long> teamB)
	{
		var steamIds = SteamIds(faction);
		var inA = teamA.Count(steamIds.Contains);
		var inB = teamB.Count(steamIds.Contains);
		return inA == inB ? null : inA > inB ? "A" : "B";
	}

	/// <summary>SteamID64s of a faction's players (those FACEIT reports).</summary>
	public static HashSet<long> SteamIds(FaceitFactionInfo faction) =>
		faction.Players
			.Select(p => long.TryParse(p.SteamId64?.Trim(), out var id) ? id : 0)
			.Where(id => id != 0)
			.ToHashSet();

	#endregion

	#region Private Methods

	/// <summary>Whether a faction name refers to the opponent (exact key, or containing a key of 3+ characters).</summary>
	private static bool NameMatches(string factionName, string opponentKey)
	{
		var name = factionName.Trim().ToLowerInvariant();
		return opponentKey.Length > 0
			&& (name == opponentKey || (opponentKey.Length >= 3 && name.Contains(opponentKey, StringComparison.Ordinal)));
	}

	#endregion
}
