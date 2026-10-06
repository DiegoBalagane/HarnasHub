#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.Tactics.Shared.Matching;

/// <summary>Loads a map's tactics as matching targets and folds matched rounds into per-tactic effectiveness.</summary>
public static class TacticEffectivenessAggregator
{
	#region Public Methods

	/// <summary>Every tactic of <paramref name="map"/> (optionally one side) with its points; a point linked to a library nade
	/// is a grenade point typed by that nade (untyped when the nade was since deleted).</summary>
	public static async Task<List<TacticTarget>> LoadTargetsAsync(IApplicationDbContext dbContext, MapName map, CancellationToken cancellationToken)
	{
		var tactics = await dbContext.Tactics.AsNoTracking()
			.Where(t => t.MapName == map)
			.Select(t => new
			{
				t.Id,
				t.Name,
				t.Side,
				Points = t.Points.Select(p => new { p.X, p.Y, p.NadeEntryId }).ToList()
			})
			.ToListAsync(cancellationToken);

		var nadeIds = tactics.SelectMany(t => t.Points).Where(p => p.NadeEntryId != null).Select(p => p.NadeEntryId!.Value).Distinct().ToList();
		var nadeTypes = await dbContext.NadeEntries.AsNoTracking()
			.Where(n => nadeIds.Contains(n.Id))
			.ToDictionaryAsync(n => n.Id, n => n.Type, cancellationToken);

		return tactics
			.Select(t => new TacticTarget(
				t.Id,
				t.Name,
				t.Side,
				t.Points.Select(p => new TacticTargetPoint(
					p.X,
					p.Y,
					p.NadeEntryId is not null,
					p.NadeEntryId is { } nadeId && nadeTypes.TryGetValue(nadeId, out var type) ? type : null)).ToList()))
			.ToList();
	}

	/// <summary>Matches every analysed match's rounds and counts, per tactic, rounds played/won and matches it showed up in;
	/// every tactic is listed, unplayed ones with zeros.</summary>
	public static TacticEffectivenessReportDto Aggregate(
		MapName map,
		bool mapCalibrated,
		IReadOnlyList<IReadOnlyList<RoundSignature>> matches,
		IReadOnlyList<TacticTarget> tactics,
		int matchesSkipped)
	{
		var played = tactics.ToDictionary(t => t.TacticId, _ => (Rounds: 0, Won: 0, Matches: 0));
		var roundsAnalyzed = 0;
		var roundsMatched = 0;

		foreach (var match in matches)
		{
			roundsAnalyzed += match.Count;
			var matched = TacticMatcher.MatchAll(match, tactics);
			roundsMatched += matched.Count;

			foreach (var group in matched.GroupBy(m => m.TacticId))
			{
				var current = played[group.Key];
				played[group.Key] = (current.Rounds + group.Count(), current.Won + group.Count(m => m.WeWon == true), current.Matches + 1);
			}
		}

		var items = tactics
			.Select(t => new TacticEffectivenessDto(t.TacticId, t.Name, t.Side, played[t.TacticId].Rounds, played[t.TacticId].Won, played[t.TacticId].Matches))
			.OrderBy(t => t.Side).ThenByDescending(t => t.RoundsPlayed).ThenBy(t => t.Name)
			.ToList();

		return new TacticEffectivenessReportDto(map, mapCalibrated, matches.Count, matchesSkipped, roundsAnalyzed, roundsMatched, items);
	}

	#endregion
}
