using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using HarnasHub.Core.Entities;
using MediatR;

namespace HarnasHub.Application.Features.AnalysisBoards.CreateBoard;

/// <summary>Handles <see cref="CreateBoardCommand"/>.</summary>
public class CreateBoardHandler(IApplicationDbContext dbContext, ICurrentUserService currentUser, IFileStorage fileStorage, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<CreateBoardCommand, ErrorOr<AnalysisBoardDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AnalysisBoardDto>> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
	{
		var board = new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = request.MapName,
			Title = request.Title,
			BackgroundImageObjectKey = request.BackgroundImageObjectKey,
			StrokesJson = request.StrokesJson,
			CreatedByUserId = currentUser.UserId,
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		};

		dbContext.AnalysisBoards.Add(board);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("analysis-boards", cancellationToken);

		string? backgroundImageUrl = null;

		if (board.BackgroundImageObjectKey is { } objectKey && fileStorage.IsConfigured)
		{
			backgroundImageUrl = await fileStorage.CreatePresignedDownloadUrlAsync(objectKey, TimeSpan.FromHours(1), cancellationToken);
		}

		return new AnalysisBoardDto(
			board.Id, board.MapName.ToString(), board.Title, board.BackgroundImageObjectKey, backgroundImageUrl, board.StrokesJson, board.UpdatedAtUtc);
	}

	#endregion
}
