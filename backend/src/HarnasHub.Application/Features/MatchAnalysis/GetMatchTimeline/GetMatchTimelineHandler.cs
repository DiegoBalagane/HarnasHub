#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetMatchTimeline;

/// <summary>Handles <see cref="GetMatchTimelineQuery"/> via <see cref="MatchTimelineLoader"/>.</summary>
public class GetMatchTimelineHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetMatchTimelineHandler> logger)
	: IRequestHandler<GetMatchTimelineQuery, ErrorOr<MatchTimelineDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public Task<ErrorOr<MatchTimelineDto>> Handle(GetMatchTimelineQuery request, CancellationToken cancellationToken) =>
		MatchTimelineLoader.LoadAsync(dbContext, fileStorage, logger, request.MatchResultId, cancellationToken);

	#endregion
}
