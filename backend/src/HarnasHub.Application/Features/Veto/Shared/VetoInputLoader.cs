using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MapPool.Shared;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.Veto.Shared;

/// <summary>The per-map veto inputs against one opponent plus the opponent's recorded tendencies.</summary>
public record VetoInputData(List<MapVetoInput> Inputs, List<OpponentMapTendencyDto> Tendencies, int RecordedOpponentVetoes);

/// <summary>Gathers the internal data <see cref="VetoScoring"/> works from — map pool, our results, head-to-head, recorded
/// vetoes and tactic counts — shared by the veto suggestion and the opponent report.</summary>
public static class VetoInputLoader
{
	#region Public Methods

	/// <summary>Builds one <see cref="MapVetoInput"/> per pool map against the opponent with normalized key <paramref name="opponentKey"/>.</summary>
	public static async Task<VetoInputData> LoadAsync(IApplicationDbContext dbContext, string opponentKey, CancellationToken cancellationToken)
	{
		var statuses = await dbContext.MapPoolEntries.ToDictionaryAsync(e => e.MapName, e => e.Status, cancellationToken);

		var results = (await dbContext.MatchResults
				.Where(m => m.MapName != null)
				.Select(m => new { m.MapName, m.Opponent, m.OurScore, m.OpponentScore })
				.ToListAsync(cancellationToken))
			.Select(m => new
			{
				Map = MapNameParser.Parse(m.MapName),
				IsAgainstOpponent = OpponentNames.ToKey(m.Opponent) == opponentKey,
				Outcome = OpponentNames.Outcome(m.OurScore, m.OpponentScore)
			})
			.Where(m => m.Map.HasValue)
			.ToList();

		var tacticCounts = await dbContext.Tactics
			.GroupBy(t => t.MapName)
			.Select(g => new { MapName = g.Key, Count = g.Count() })
			.ToDictionaryAsync(x => x.MapName, x => x.Count, cancellationToken);

		// Only the opponent's own steps from vetoes of events against them — our steps say nothing about them.
		var opponentSteps = await dbContext.EventVetoSteps
			.Where(s => s.Actor == VetoActor.Opponent)
			.Join(
				dbContext.Events.Where(e => e.Opponent != null && e.Opponent.Trim().ToLower() == opponentKey),
				s => s.EventId,
				e => e.Id,
				(s, e) => new { s.EventId, s.MapName, s.Action })
			.ToListAsync(cancellationToken);

		var inputs = Enum.GetValues<MapName>().Select(map =>
		{
			var onMap = results.Where(r => r.Map == map).ToList();
			var headToHead = onMap.Where(r => r.IsAgainstOpponent).ToList();

			return new MapVetoInput(
				map,
				statuses.TryGetValue(map, out var status) ? status : null,
				onMap.Sum(r => r.Outcome.Wins),
				onMap.Sum(r => r.Outcome.Losses),
				onMap.Sum(r => r.Outcome.Draws),
				headToHead.Sum(r => r.Outcome.Wins),
				headToHead.Sum(r => r.Outcome.Losses),
				opponentSteps.Count(s => s.MapName == map && s.Action == VetoAction.Pick),
				opponentSteps.Count(s => s.MapName == map && s.Action == VetoAction.Ban),
				tacticCounts.GetValueOrDefault(map));
		}).ToList();

		var tendencies = inputs
			.Where(i => i.OpponentPicks + i.OpponentBans > 0)
			.OrderByDescending(i => i.OpponentPicks)
			.ThenByDescending(i => i.OpponentBans)
			.Select(i => new OpponentMapTendencyDto(i.MapName.ToString(), i.OpponentPicks, i.OpponentBans))
			.ToList();

		return new VetoInputData(inputs, tendencies, opponentSteps.Select(s => s.EventId).Distinct().Count());
	}

	#endregion
}
