using HarnasHub.Core.Enums;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>One opponent player's scoreboard line on one pool map.</summary>
public record PlayerGameLine(
	string PlayerId,
	string Nickname,
	MapName Map,
	int Kills,
	int Deaths,
	double? Adr,
	double? HeadshotPercent,
	int MultiKills);

/// <summary>Picks the most dangerous opponent players per map from their scoreboard lines (team and solo games alike).</summary>
public static class PlayersToWatchCalculator
{
	#region Public Fields

	/// <summary>Players with fewer games on a map are left out — one lucky game isn't a threat profile.</summary>
	public const int MinGames = 2;

	/// <summary>How many players are listed per map.</summary>
	public const int TopPerMap = 3;

	#endregion

	#region Public Methods

	/// <summary>Top players per map ranked by ADR (K/D when ADR is missing); maps ordered by how much data they have.</summary>
	public static List<MapPlayersToWatchDto> Calculate(IEnumerable<PlayerGameLine> lines) =>
		lines
			.GroupBy(l => l.Map)
			.Select(map => (Map: map.Key, Lines: map.Count(), Players: map
				.GroupBy(l => l.PlayerId)
				.Where(p => p.Count() >= MinGames)
				.Select(ToPlayer)
				.OrderByDescending(p => p.Adr ?? 0)
				.ThenByDescending(p => p.KdRatio)
				.Take(TopPerMap)
				.ToList()))
			.Where(x => x.Players.Count > 0)
			.OrderByDescending(x => x.Lines)
			.ThenBy(x => x.Map)
			.Select(x => new MapPlayersToWatchDto(x.Map.ToString(), x.Players))
			.ToList();

	#endregion

	#region Private Methods

	/// <summary>Averages one player's lines on a map; the latest nickname wins.</summary>
	private static PlayerToWatchDto ToPlayer(IGrouping<string, PlayerGameLine> games)
	{
		var list = games.ToList();
		var kills = list.Sum(g => g.Kills);
		var deaths = list.Sum(g => g.Deaths);
		var adrs = list.Where(g => g.Adr.HasValue).Select(g => g.Adr!.Value).ToList();
		var headshots = list.Where(g => g.HeadshotPercent.HasValue).Select(g => g.HeadshotPercent!.Value).ToList();

		return new PlayerToWatchDto(
			games.Key,
			list[^1].Nickname,
			list.Count,
			Math.Round(deaths == 0 ? kills : (double)kills / deaths, 2),
			adrs.Count == 0 ? null : Math.Round(adrs.Average(), 1),
			headshots.Count == 0 ? null : Math.Round(headshots.Average(), 1),
			Math.Round((double)list.Sum(g => g.MultiKills) / list.Count, 2));
	}

	#endregion
}
