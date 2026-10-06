using HarnasHub.Application.Abstractions;

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Reads a player's paged FACEIT history and decides which rooms get their scoreboards fetched.</summary>
public static class FaceitHistoryPager
{
	#region Public Methods

	/// <summary>Up to <see cref="FaceitSync.MaxHistoryPages"/> pages of the player's history in the window, newest first.</summary>
	public static async Task<List<FaceitHistoryItem>> LoadAsync(
		IFaceitClient client,
		string playerId,
		DateTime since,
		CancellationToken cancellationToken)
	{
		var items = new List<FaceitHistoryItem>();
		for (var page = 0; page < FaceitSync.MaxHistoryPages; page++)
		{
			var batch = await client.GetPlayerHistoryAsync(playerId, since, FaceitSync.HistoryPageSize, cancellationToken, page * FaceitSync.HistoryPageSize);
			items.AddRange(batch);
			if (batch.Count < FaceitSync.HistoryPageSize)
			{
				break;
			}
		}

		return items;
	}

	/// <summary>Every non-matchmaking room (championship such as ESEA League, hub — where team games live) first, then the latest
	/// <see cref="FaceitSync.HistoryLimit"/> matchmaking games, so a busy PUG player can't push their league games out of the fetch.</summary>
	public static IEnumerable<FaceitHistoryItem> SelectToFetch(List<FaceitHistoryItem> newestFirst)
	{
		var organised = newestFirst.Where(h => !IsMatchmaking(h.CompetitionType)).ToList();
		var matchmaking = newestFirst.Where(h => IsMatchmaking(h.CompetitionType)).Take(FaceitSync.HistoryLimit);
		return organised.Concat(matchmaking).DistinctBy(h => h.MatchId);
	}

	/// <summary>Whether FACEIT's competition type is the public matchmaking queue.</summary>
	public static bool IsMatchmaking(string? competitionType) =>
		string.Equals(competitionType, "matchmaking", StringComparison.OrdinalIgnoreCase);

	#endregion
}
