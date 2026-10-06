#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMatchInsights;

/// <summary>Handles <see cref="GetMatchInsightsQuery"/> by loading the timeline and running <see cref="MatchInsightRules"/>.</summary>
public class GetMatchInsightsHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetMatchInsightsHandler> logger)
	: IRequestHandler<GetMatchInsightsQuery, ErrorOr<IReadOnlyList<MatchInsightDto>>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<IReadOnlyList<MatchInsightDto>>> Handle(GetMatchInsightsQuery request, CancellationToken cancellationToken)
	{
		var timeline = await MatchTimelineLoader.LoadAsync(dbContext, fileStorage, logger, request.MatchResultId, cancellationToken);
		if (timeline.IsError)
		{
			return timeline.Errors;
		}

		return ErrorOrFactory.From(MatchInsightRules.Build(timeline.Value));
	}

	#endregion
}
