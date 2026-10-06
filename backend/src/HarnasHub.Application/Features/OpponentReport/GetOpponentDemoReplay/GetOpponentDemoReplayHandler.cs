#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentDemoReplay;

/// <summary>Handles <see cref="GetOpponentDemoReplayQuery"/> through <see cref="RoundReplayLoader.ForOpponentDemoAsync"/>.</summary>
public class GetOpponentDemoReplayHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, ILogger<GetOpponentDemoReplayHandler> logger)
	: IRequestHandler<GetOpponentDemoReplayQuery, ErrorOr<RoundReplayDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public Task<ErrorOr<RoundReplayDto>> Handle(GetOpponentDemoReplayQuery request, CancellationToken cancellationToken) =>
		RoundReplayLoader.ForOpponentDemoAsync(dbContext, fileStorage, logger, request.OpponentDemoId, request.RoundNumber, cancellationToken);

	#endregion
}
