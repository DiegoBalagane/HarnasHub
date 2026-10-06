#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Tactics.Shared.Matching;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.Tactics.GetMatchTacticMatches;

/// <summary>Handles <see cref="GetMatchTacticMatchesQuery"/>: loads the stored timeline, extracts our round signatures
/// (falling back to the round-1 T team when "us" is unresolved) and runs <see cref="TacticMatcher"/> on read.</summary>
public class GetMatchTacticMatchesHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetMatchTacticMatchesHandler> logger)
	: IRequestHandler<GetMatchTacticMatchesQuery, ErrorOr<MatchTacticMatchesDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<MatchTacticMatchesDto>> Handle(GetMatchTacticMatchesQuery request, CancellationToken cancellationToken)
	{
		var loaded = await MatchTimelineLoader.LoadRawAsync(dbContext, fileStorage, logger, request.MatchResultId, cancellationToken);
		if (loaded.IsError)
		{
			return loaded.Errors;
		}

		var timeline = loaded.Value.Stored.Timeline;
		var resolved = loaded.Value.OurTeam.Count > 0;
		var hasPositions = timeline.Positions.Count > 0;

		if (timeline.MapName is not { } map || !MapRadarSupport.HasVerifiedRadar(map))
		{
			return new MatchTacticMatchesDto(false, hasPositions, resolved, []);
		}

		var ours = resolved
			? loaded.Value.OurTeam
			: timeline.Rounds.OrderBy(r => r.Number).FirstOrDefault()?.TerroristSteamIds ?? [];
		var targets = await TacticEffectivenessAggregator.LoadTargetsAsync(dbContext, map, cancellationToken);
		var rounds = TacticMatcher.MatchAll(RoundSignatureExtractor.Extract(timeline, ours.ToList()), targets)
			.Select(m => new RoundTacticMatchDto(m.RoundNumber, m.Side, m.TacticId, m.TacticName, (int)Math.Round(m.Score * 100)))
			.ToList();

		return new MatchTacticMatchesDto(true, hasPositions, resolved, rounds);
	}

	#endregion
}
