#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Tendencies;

/// <summary>Reads the stored facts of every resolved opponent demo and aggregates them per map — database only, so it is
/// cheap enough to run on every report read (tendencies are never part of the FACEIT snapshot).</summary>
public static class OpponentTendencyLoader
{
	#region Public Methods

	/// <summary>Tendencies per pool map with at least one resolved demo, most demos first; empty without demos.</summary>
	public static async Task<List<MapTendenciesDto>> LoadAsync(IApplicationDbContext dbContext, string opponentKey, CancellationToken cancellationToken)
	{
		var rows = await dbContext.OpponentDemoAnalyses.AsNoTracking()
			.Where(a => a.OpponentKey == opponentKey && a.MapName != null && a.FactsJson != null)
			.OrderBy(a => a.CreatedAtUtc)
			.Select(a => new { a.MapName, a.FactsJson })
			.ToListAsync(cancellationToken);

		var maps = rows
			.Select(r => (Map: r.MapName!.Value, Facts: OpponentFactsSerializer.Deserialize(r.FactsJson)))
			.Where(r => r.Facts is not null)
			.GroupBy(r => r.Map)
			.Select(g => OpponentTendencyAggregator.Aggregate(g.Key, g.Select(r => r.Facts!).ToList()))
			.OrderByDescending(t => t.Demos)
			.ThenBy(t => t.MapName, StringComparer.Ordinal)
			.ToList();

		return await OpponentPlayerNames.ResolveTendenciesAsync(dbContext, maps, cancellationToken);
	}

	#endregion
}
