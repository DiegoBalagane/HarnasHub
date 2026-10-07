#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using HarnasHub.Application.Features.Stats.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.Stats.GetAdvancedStats;

/// <summary>Handles <see cref="GetAdvancedStatsQuery"/>: picks the newest analysed matches (at most <see cref="MaxMatches"/>),
/// loads their stored timelines one by one and folds them with <see cref="AdvancedStatsAggregator"/>.</summary>
public class GetAdvancedStatsHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetAdvancedStatsHandler> logger)
	: IRequestHandler<GetAdvancedStatsQuery, ErrorOr<AdvancedStatsDto>>
{
	#region Public Fields

	/// <summary>How many of the newest analysed matches are aggregated at most (each one means a timeline download).</summary>
	public const int MaxMatches = 30;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<AdvancedStatsDto>> Handle(GetAdvancedStatsQuery request, CancellationToken cancellationToken)
	{
		var take = Math.Clamp(request.Last ?? MaxMatches, 1, MaxMatches);

		var matches = dbContext.MatchResults.AsNoTracking()
			.Where(m => dbContext.MatchDemoAnalyses.Any(a => a.MatchResultId == m.Id));

		if (request.Category is { } category)
		{
			matches = matches.Where(m => m.Category == category);
		}

		if (request.Map is { } map)
		{
			var mapName = map.ToString().ToLower();
			matches = matches.Where(m => m.MapName != null && m.MapName.ToLower() == mapName);
		}

		var results = await matches.OrderByDescending(m => m.PlayedAtUtc).Take(take).ToListAsync(cancellationToken);
		var users = await dbContext.Users.AsNoTracking()
			.Where(u => u.ShowInStats && u.SteamId64 != null)
			.ToListAsync(cancellationToken);

		if (results.Count == 0 || users.Count == 0 || !fileStorage.IsConfigured)
		{
			return AdvancedStatsAggregator.Aggregate([], users, []);
		}

		var inputs = new List<AdvancedMatchInput>();
		var skipped = 0;

		foreach (var result in results)
		{
			var loaded = await MatchTimelineLoader.LoadRawAsync(dbContext, fileStorage, logger, result.Id, cancellationToken);
			if (loaded.IsError)
			{
				skipped++;
				continue;
			}

			var timeline = loaded.Value.Stored.Timeline;
			var context = AnalysisContext.Create(timeline, loaded.Value.OurTeam.ToList());
			inputs.Add(new AdvancedMatchInput(result, context, timeline.MapName?.ToString() ?? result.MapName ?? "?"));
		}

		var ids = inputs.Select(i => i.Result.Id).ToList();
		var userIds = users.Select(u => u.Id).ToList();
		var stats = await dbContext.PlayerMatchStats.AsNoTracking()
			.Where(s => ids.Contains(s.MatchResultId) && s.UserId != null && userIds.Contains(s.UserId.Value))
			.ToListAsync(cancellationToken);

		return AdvancedStatsAggregator.Aggregate(inputs, users, stats, skipped);
	}

	#endregion
}
