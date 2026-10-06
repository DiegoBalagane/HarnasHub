#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using HarnasHub.Application.Features.MatchAnalysis.Shared.Replay;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.AnalysisBoards.CreateBoardFromRound;

/// <summary>Handles <see cref="CreateBoardFromRoundCommand"/>: loads the round replay, draws it with
/// <see cref="RoundBoardBuilder"/> and stores the result as a regular <see cref="AnalysisBoard"/> over the built-in radar.</summary>
public class CreateBoardFromRoundHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	ICurrentUserService currentUser,
	IRealtimeNotifier realtimeNotifier,
	ILogger<CreateBoardFromRoundHandler> logger)
	: IRequestHandler<CreateBoardFromRoundCommand, ErrorOr<AnalysisBoardDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<AnalysisBoardDto>> Handle(CreateBoardFromRoundCommand request, CancellationToken cancellationToken)
	{
		var loaded = request.Source == ReplaySource.Match
			? await RoundReplayLoader.ForMatchAsync(dbContext, fileStorage, logger, request.SourceId, request.RoundNumber, cancellationToken)
			: await RoundReplayLoader.ForOpponentDemoAsync(dbContext, fileStorage, logger, request.SourceId, request.RoundNumber, cancellationToken);
		if (loaded.IsError)
		{
			return loaded.Errors;
		}

		var replay = loaded.Value;
		if (replay.PositionsStatus == ReplayPositionsStatus.UncalibratedMap || !Enum.TryParse<MapName>(replay.MapName, out var mapName))
		{
			return AnalysisBoardErrors.MapUncalibrated;
		}

		if (replay.PositionsStatus == ReplayPositionsStatus.NotRecorded)
		{
			return AnalysisBoardErrors.PositionsNotRecorded;
		}

		var second = Math.Min(request.Second, replay.DurationSeconds);
		var now = DateTime.UtcNow;
		var board = new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = mapName,
			Title = RoundBoardBuilder.Title(replay, second),
			StrokesJson = RoundBoardBuilder.ToJson(RoundBoardBuilder.Build(replay, second)),
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = now,
			UpdatedAtUtc = now
		};

		dbContext.AnalysisBoards.Add(board);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("analysis-boards", cancellationToken);

		return new AnalysisBoardDto(board.Id, board.MapName.ToString(), board.Title, null, null, board.StrokesJson, board.UpdatedAtUtc);
	}

	#endregion
}
