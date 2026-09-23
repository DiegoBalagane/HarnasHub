using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.AnalysisBoards.UpdateBoard;

/// <summary>Handles <see cref="UpdateBoardCommand"/>.</summary>
public class UpdateBoardHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<UpdateBoardCommand, ErrorOr<AnalysisBoardDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AnalysisBoardDto>> Handle(UpdateBoardCommand request, CancellationToken cancellationToken)
	{
		var board = await dbContext.AnalysisBoards.FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken);

		if (board is null)
		{
			return AnalysisBoardErrors.BoardNotFound;
		}

		// The old background is only ever referenced by this one board, so a replacement or a clear leaves it
		// orphaned in the bucket unless it's removed here.
		if (board.BackgroundImageObjectKey is { } oldKey && oldKey != request.BackgroundImageObjectKey && fileStorage.IsConfigured)
		{
			await fileStorage.DeleteAsync(oldKey, cancellationToken);
		}

		board.Title = request.Title;
		board.BackgroundImageObjectKey = request.BackgroundImageObjectKey;
		board.StrokesJson = request.StrokesJson;
		board.UpdatedAtUtc = DateTime.UtcNow;

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
