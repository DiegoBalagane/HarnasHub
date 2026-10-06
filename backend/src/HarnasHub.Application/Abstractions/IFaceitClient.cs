namespace HarnasHub.Application.Abstractions;

/// <summary>Read-only access to the FACEIT Data API v4. Lookups return null when FACEIT has no such resource (404); transport
/// failures that survive the client's retries surface as <see cref="HttpRequestException"/>.</summary>
public interface IFaceitClient
{
	/// <summary>Whether an API key is configured — when false every call would be rejected, so handlers bail out early.</summary>
	bool IsConfigured { get; }

	/// <summary>Finds a player by FACEIT nickname (case-insensitive on FACEIT's side).</summary>
	Task<FaceitPlayerInfo?> GetPlayerByNicknameAsync(string nickname, CancellationToken cancellationToken);

	/// <summary>Finds the FACEIT account linked to a CS2 SteamID64.</summary>
	Task<FaceitPlayerInfo?> GetPlayerBySteamIdAsync(string steamId64, CancellationToken cancellationToken);

	/// <summary>Loads a player profile by FACEIT player id.</summary>
	Task<FaceitPlayerInfo?> GetPlayerAsync(string playerId, CancellationToken cancellationToken);

	/// <summary>Loads a FACEIT team with its members.</summary>
	Task<FaceitTeamInfo?> GetTeamAsync(string teamId, CancellationToken cancellationToken);

	/// <summary>Loads a match room with both factions' rosters.</summary>
	Task<FaceitMatchInfo?> GetMatchAsync(string matchId, CancellationToken cancellationToken);

	/// <summary>A player's CS2 match history since <paramref name="fromUtc"/>, newest first, at most <paramref name="limit"/> entries.</summary>
	Task<IReadOnlyList<FaceitHistoryItem>> GetPlayerHistoryAsync(
		string playerId,
		DateTime fromUtc,
		int limit,
		CancellationToken cancellationToken);

	/// <summary>Per-map scoreboards of a finished match; empty when FACEIT has no statistics for it.</summary>
	Task<IReadOnlyList<FaceitMapStats>> GetMatchStatsAsync(string matchId, CancellationToken cancellationToken);
}
