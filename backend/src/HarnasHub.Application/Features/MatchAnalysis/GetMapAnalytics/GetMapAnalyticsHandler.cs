#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Analysis;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMapAnalytics;

/// <summary>Handles <see cref="GetMapAnalyticsQuery"/>: picks the newest analysed matches on the map (at most <see cref="MaxMatches"/>),
/// loads each stored timeline and folds them with <see cref="MapAnalyticsAggregator"/>.</summary>
public class GetMapAnalyticsHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetMapAnalyticsHandler> logger)
	: IRequestHandler<GetMapAnalyticsQuery, ErrorOr<MapAnalyticsDto>>
{
	#region Public Fields

	/// <summary>How many of the newest analysed matches per map are aggregated.</summary>
	public const int MaxMatches = 20;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<MapAnalyticsDto>> Handle(GetMapAnalyticsQuery request, CancellationToken cancellationToken)
	{
		var mapName = request.Map.ToString().ToLower();

		var candidates = await (
				from result in dbContext.MatchResults.AsNoTracking()
				join analysis in dbContext.MatchDemoAnalyses.AsNoTracking() on result.Id equals analysis.MatchResultId
				where result.MapName != null && result.MapName.ToLower() == mapName
				orderby result.PlayedAtUtc descending
				select new { result.OurScore, result.OpponentScore, analysis.ObjectKey, analysis.OurTeamSteamIds, analysis.ExcludedSteamIds })
			.Take(MaxMatches)
			.ToListAsync(cancellationToken);

		if (candidates.Count == 0 || !fileStorage.IsConfigured)
		{
			return MapAnalyticsAggregator.Aggregate(request.Map, []);
		}

		var roster = await MatchTimelineAttacher.LoadRosterSteamIdsAsync(dbContext, cancellationToken);
		var loaded = new List<LoadedMatchTimeline>();
		var failed = 0;

		foreach (var candidate in candidates)
		{
			try
			{
				var stored = await MatchTimelineStorage.LoadAsync(fileStorage, candidate.ObjectKey, cancellationToken);
				stored = stored with { Timeline = RoundParticipants.Exclude(stored.Timeline, candidate.ExcludedSteamIds) };
				IReadOnlyList<long> ourTeam = candidate.OurTeamSteamIds.Count > 0
					? candidate.OurTeamSteamIds
					: TimelineTeamResolver.ResolveOurTeam(stored.Timeline.Rounds, roster, candidate.OurScore, candidate.OpponentScore);
				loaded.Add(new LoadedMatchTimeline(stored, ourTeam));
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				failed++;
				logger.LogWarning(ex, "Nie udało się wczytać osi czasu {ObjectKey} do statystyk mapy {Map}", candidate.ObjectKey, request.Map);
			}
		}

		return MapAnalyticsAggregator.Aggregate(request.Map, loaded, failed);
	}

	#endregion
}
