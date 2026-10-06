#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.Tactics.GetTacticEffectiveness;

/// <summary>Handles <see cref="GetTacticEffectivenessQuery"/>: takes the newest analysed matches on the map (at most
/// <see cref="MaxMatches"/>), gets their round signatures through <see cref="RoundSignatureCache"/> (each timeline is
/// downloaded once per process) and matches them against the current tactics on every read, so editing a tactic is
/// reflected immediately without any stored results.</summary>
public class GetTacticEffectivenessHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	RoundSignatureCache cache,
	ILogger<GetTacticEffectivenessHandler> logger)
	: IRequestHandler<GetTacticEffectivenessQuery, ErrorOr<TacticEffectivenessReportDto>>
{
	#region Public Fields

	/// <summary>How many of the newest analysed matches per map are matched.</summary>
	public const int MaxMatches = 20;

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<TacticEffectivenessReportDto>> Handle(GetTacticEffectivenessQuery request, CancellationToken cancellationToken)
	{
		var targets = await TacticEffectivenessAggregator.LoadTargetsAsync(dbContext, request.Map, cancellationToken);
		if (!MapRadarSupport.HasVerifiedRadar(request.Map))
		{
			return TacticEffectivenessAggregator.Aggregate(request.Map, false, [], targets, 0);
		}

		var mapName = request.Map.ToString().ToLower();
		var candidates = await (
				from result in dbContext.MatchResults.AsNoTracking()
				join analysis in dbContext.MatchDemoAnalyses.AsNoTracking() on result.Id equals analysis.MatchResultId
				where result.MapName != null && result.MapName.ToLower() == mapName
				orderby result.PlayedAtUtc descending
				select new { result.OurScore, result.OpponentScore, analysis.ObjectKey, analysis.CreatedAtUtc, analysis.OurTeamSteamIds })
			.Take(MaxMatches)
			.ToListAsync(cancellationToken);

		if (candidates.Count == 0 || !fileStorage.IsConfigured)
		{
			return TacticEffectivenessAggregator.Aggregate(request.Map, true, [], targets, candidates.Count);
		}

		HashSet<long>? roster = null;
		var matches = new List<IReadOnlyList<RoundSignature>>();
		var skipped = 0;

		foreach (var candidate in candidates)
		{
			try
			{
				var key = RoundSignatureCache.Key(candidate.ObjectKey, candidate.CreatedAtUtc, candidate.OurTeamSteamIds);
				var signatures = await cache.GetOrAddAsync(key, async () =>
				{
					var stored = await MatchTimelineStorage.LoadAsync(fileStorage, candidate.ObjectKey, cancellationToken);
					IReadOnlyList<long> ourTeam = candidate.OurTeamSteamIds;
					if (ourTeam.Count == 0)
					{
						roster ??= await MatchTimelineAttacher.LoadRosterSteamIdsAsync(dbContext, cancellationToken);
						ourTeam = TimelineTeamResolver.ResolveOurTeam(stored.Timeline.Rounds, roster, candidate.OurScore, candidate.OpponentScore);
					}

					return RoundSignatureExtractor.Extract(stored.Timeline, ourTeam.ToList());
				});

				if (signatures.Count == 0)
				{
					skipped++;
					continue;
				}

				matches.Add(signatures);
			}
			catch (Exception ex) when (ex is not OperationCanceledException)
			{
				skipped++;
				logger.LogWarning(ex, "Nie udało się wczytać osi czasu {ObjectKey} do skuteczności taktyk na mapie {Map}", candidate.ObjectKey, request.Map);
			}
		}

		return TacticEffectivenessAggregator.Aggregate(request.Map, true, matches, targets, skipped);
	}

	#endregion
}
