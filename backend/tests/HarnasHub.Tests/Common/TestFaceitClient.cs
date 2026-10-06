using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>In-memory <see cref="IFaceitClient"/>: tests seed players, teams, matches, histories and stats, and can make every call throw.</summary>
public class TestFaceitClient(bool isConfigured = true) : IFaceitClient
{
	#region Public Properties

	public bool IsConfigured { get; } = isConfigured;

	public Dictionary<string, FaceitPlayerInfo> PlayersByNickname { get; } = new(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, FaceitPlayerInfo> PlayersBySteamId { get; } = [];

	public Dictionary<string, FaceitPlayerInfo> PlayersById { get; } = [];

	public Dictionary<string, FaceitTeamInfo> Teams { get; } = [];

	public Dictionary<string, FaceitMatchInfo> Matches { get; } = [];

	public Dictionary<string, List<FaceitHistoryItem>> Histories { get; } = [];

	public Dictionary<string, List<FaceitMapStats>> Stats { get; } = [];

	/// <summary>When set, every call throws it — simulates FACEIT being down after retries.</summary>
	public Exception? ThrowOnCall { get; set; }

	/// <summary>Match ids whose stats were requested, in order.</summary>
	public List<string> StatsRequests { get; } = [];

	/// <summary>Player ids whose history was requested, in order.</summary>
	public List<string> HistoryRequests { get; } = [];

	/// <summary>Lifetime per-map stats per player id.</summary>
	public Dictionary<string, List<FaceitLifetimeMapStats>> LifetimeMapStats { get; } = [];

	/// <summary>Player ids whose lifetime map stats were requested, in order.</summary>
	public List<string> MapStatsRequests { get; } = [];

	#endregion

	#region Public Methods

	public Task<FaceitPlayerInfo?> GetPlayerByNicknameAsync(string nickname, CancellationToken cancellationToken) =>
		Respond(PlayersByNickname.GetValueOrDefault(nickname));

	public Task<FaceitPlayerInfo?> GetPlayerBySteamIdAsync(string steamId64, CancellationToken cancellationToken) =>
		Respond(PlayersBySteamId.GetValueOrDefault(steamId64));

	public Task<FaceitPlayerInfo?> GetPlayerAsync(string playerId, CancellationToken cancellationToken) =>
		Respond(PlayersById.GetValueOrDefault(playerId));

	public Task<FaceitTeamInfo?> GetTeamAsync(string teamId, CancellationToken cancellationToken) =>
		Respond(Teams.GetValueOrDefault(teamId));

	public Task<FaceitMatchInfo?> GetMatchAsync(string matchId, CancellationToken cancellationToken) =>
		Respond(Matches.GetValueOrDefault(matchId));

	public Task<IReadOnlyList<FaceitHistoryItem>> GetPlayerHistoryAsync(string playerId, DateTime fromUtc, int limit, CancellationToken cancellationToken, int offset = 0)
	{
		HistoryRequests.Add(playerId);
		IReadOnlyList<FaceitHistoryItem> items = Histories.GetValueOrDefault(playerId)?.Skip(offset).Take(limit).ToList() ?? [];
		return Respond(items)!;
	}

	public Task<IReadOnlyList<FaceitLifetimeMapStats>> GetPlayerMapStatsAsync(string playerId, CancellationToken cancellationToken)
	{
		MapStatsRequests.Add(playerId);
		IReadOnlyList<FaceitLifetimeMapStats> maps = LifetimeMapStats.GetValueOrDefault(playerId) ?? [];
		return Respond(maps)!;
	}

	public Task<IReadOnlyList<FaceitMapStats>> GetMatchStatsAsync(string matchId, CancellationToken cancellationToken)
	{
		StatsRequests.Add(matchId);
		IReadOnlyList<FaceitMapStats> maps = Stats.GetValueOrDefault(matchId) ?? [];
		return Respond(maps)!;
	}

	/// <summary>Builds a two-team map scoreboard: team 1 wins <paramref name="team1Score"/>–<paramref name="team2Score"/> when higher.</summary>
	public static FaceitMapStats MapStats(string mapName, IEnumerable<string> team1, IEnumerable<string> team2, int team1Score = 13, int team2Score = 7) =>
		new(1, mapName,
		[
			new FaceitTeamMapStats("faction1", "Team 1", team1Score, team1Score > team2Score, team1.Select(Line).ToList()),
			new FaceitTeamMapStats("faction2", "Team 2", team2Score, team2Score > team1Score, team2.Select(Line).ToList())
		]);

	#endregion

	#region Private Methods

	private Task<T?> Respond<T>(T? value) where T : class =>
		ThrowOnCall is null ? Task.FromResult(value) : Task.FromException<T?>(ThrowOnCall);

	private static FaceitPlayerMapStats Line(string playerId) =>
		new(playerId, $"nick-{playerId}", 20, 15, 5, 85.5, 50, 1, 0, 0, 2);

	#endregion
}
