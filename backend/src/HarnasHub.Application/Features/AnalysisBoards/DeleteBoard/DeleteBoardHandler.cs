using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.AnalysisBoards.DeleteBoard;

/// <summary>Handles <see cref="DeleteBoardCommand"/>.</summary>
public class DeleteBoardHandler(IApplicationDbContext dbContext, IFileStorage fileStorage, IRealtimeNotifier realtimeNotifier)
	: IRequestHandler<DeleteBoardCommand, ErrorOr<Success>>
{
	#region Public Methods

	public async Task<ErrorOr<Success>> Handle(DeleteBoardCommand request, CancellationToken cancellationToken)
	{
		var board = await dbContext.AnalysisBoards.FirstOrDefaultAsync(b => b.Id == request.BoardId, cancellationToken);

		if (board is null)
		{
			return AnalysisBoardErrors.BoardNotFound;
		}

		if (board.BackgroundImageObjectKey is { } objectKey && fileStorage.IsConfigured)
		{
			await fileStorage.DeleteAsync(objectKey, cancellationToken);
		}

		dbContext.AnalysisBoards.Remove(board);
		await dbContext.SaveChangesAsync(cancellationToken);
		await realtimeNotifier.NotifyAsync("analysis-boards", cancellationToken);

		return Result.Success;
	}

	#endregion
}
