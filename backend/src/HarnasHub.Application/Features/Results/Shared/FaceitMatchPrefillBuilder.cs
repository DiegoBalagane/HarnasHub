#region Usings

using HarnasHub.Application.Common.Faceit;
using HarnasHub.Application.Features.OpponentReport.Shared;

#endregion

namespace HarnasHub.Application.Features.Results.Shared;

/// <summary>Turns a recognised FACEIT match plus the demo's round-1 rosters into <see cref="FaceitMatchPrefillDto"/> (pure).</summary>
public static class FaceitMatchPrefillBuilder
{
	#region Public Methods

	/// <summary>Builds the prefill; the map comes from the series pick of the demo's map number (or the only pick).</summary>
	public static FaceitMatchPrefillDto Build(FaceitDemoMatch found, IReadOnlyCollection<long> teamA, IReadOnlyCollection<long> teamB)
	{
		var match = found.Match;
		var factions = match.Factions
			.Select((faction, index) => new FaceitFactionPrefillDto(
				faction.Name,
				FaceitFactionMatcher.DemoTeam(faction, teamA, teamB),
				faction.Players.Select(p => p.PlayerId).ToList(),
				faction.Players.Select(p => p.Nickname).ToList(),
				found.LinkedOpponentNames.ElementAtOrDefault(index)))
			.ToList();

		return new FaceitMatchPrefillDto(
			match.MatchId,
			match.CompetitionName,
			FaceitCompetitionCategory.Map(match.CompetitionType, match.CompetitionName),
			match.StartedAtUtc ?? match.FinishedAtUtc,
			PickedMap(match.PickedMaps, found.Reference.MapNumber),
			found.OurFactionIndex,
			factions);
	}

	#endregion

	#region Private Methods

	/// <summary>Pool map of the picked map for <paramref name="mapNumber"/>; the only pick when the number is unknown/out of range.</summary>
	private static string? PickedMap(IReadOnlyList<string> picks, int? mapNumber)
	{
		var raw = mapNumber is { } number && number <= picks.Count
			? picks[number - 1]
			: picks.Count == 1 ? picks[0] : null;
		return TeamMatchDetector.ParseMap(raw)?.ToString();
	}

	#endregion
}
