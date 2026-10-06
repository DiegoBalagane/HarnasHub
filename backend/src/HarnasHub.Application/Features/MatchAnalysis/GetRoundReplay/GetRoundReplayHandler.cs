#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.GetRoundReplay;

/// <summary>Handles <see cref="GetRoundReplayQuery"/> through <see cref="RoundReplayLoader.ForMatchAsync"/>.</summary>
public class GetRoundReplayHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetRoundReplayHandler> logger)
	: IRequestHandler<GetRoundReplayQuery, ErrorOr<RoundReplayDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public Task<ErrorOr<RoundReplayDto>> Handle(GetRoundReplayQuery request, CancellationToken cancellationToken) =>
		RoundReplayLoader.ForMatchAsync(dbContext, fileStorage, logger, request.MatchResultId, request.RoundNumber, cancellationToken);

	#endregion
}
