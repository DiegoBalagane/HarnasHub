#region Usings

using HarnasHub.Core.Entities;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Finds a FACEIT team's official games: in championship rooms (ESEA League, cups) and team hubs FACEIT uses the team id as
/// the faction id, so a cached map whose faction id equals the linked team id was played by that team — whoever stood in it.</summary>
public static class OfficialMatchDetector
{
	#region Public Fields

	/// <summary>The team's side must have at least this many players — keeps 2v2 wingman cups of the same team out of 5v5 numbers.</summary>
	public const int MinSidePlayers = 4;

	#endregion

	#region Public Methods

	/// <summary>The side (1 or 2) whose faction id is <paramref name="faceitTeamId"/>, or null when neither is (or the side is too small).</summary>
	public static int? FindSide(FaceitMatch match, string? faceitTeamId)
	{
		if (string.IsNullOrWhiteSpace(faceitTeamId))
		{
			return null;
		}

		var side = string.Equals(match.Team1FactionId, faceitTeamId, StringComparison.OrdinalIgnoreCase) ? 1
			: string.Equals(match.Team2FactionId, faceitTeamId, StringComparison.OrdinalIgnoreCase) ? 2
			: (int?)null;
		var players = side == 1 ? match.Team1PlayerIds : match.Team2PlayerIds;
		return side is not null && players.Count >= MinSidePlayers ? side : null;
	}

	/// <summary>All official games of the team among <paramref name="matches"/>, newest first, tagged with their ESEA season.</summary>
	public static List<TeamGame> Detect(IEnumerable<FaceitMatch> matches, string? faceitTeamId) =>
		string.IsNullOrWhiteSpace(faceitTeamId)
			? []
			: matches
				.Select(match => (Match: match, Side: FindSide(match, faceitTeamId)))
				.Where(x => x.Side.HasValue)
				.Select(x => TeamMatchDetector.ToTeamGame(x.Match, x.Side!.Value) with
				{
					Official = true,
					Season = EseaSeasonParser.Parse(x.Match.CompetitionName)
				})
				.OrderByDescending(g => g.PlayedAtUtc)
				.ToList();

	#endregion
}
