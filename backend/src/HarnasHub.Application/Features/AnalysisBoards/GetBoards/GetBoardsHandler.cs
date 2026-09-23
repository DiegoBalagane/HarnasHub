using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.AnalysisBoards.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HarnasHub.Application.Features.AnalysisBoards.GetBoards;

/// <summary>Handles <see cref="GetBoardsQuery"/>, resolving each board's background object key into a freshly
/// presigned GET URL — presigned once per request rather than stored, so it's never served stale/expired.</summary>
public class GetBoardsHandler(IApplicationDbContext dbContext, IFileStorage fileStorage)
	: IRequestHandler<GetBoardsQuery, ErrorOr<List<AnalysisBoardDto>>>
{
	#region Public Methods

	public async Task<ErrorOr<List<AnalysisBoardDto>>> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
	{
		var query = dbContext.AnalysisBoards.AsQueryable();

		if (request.MapName is { } mapName)
		{
			query = query.Where(b => b.MapName == mapName);
		}

		var boards = await query
			.OrderByDescending(b => b.UpdatedAtUtc)
			.ToListAsync(cancellationToken);

		var dtos = new List<AnalysisBoardDto>(boards.Count);

		foreach (var board in boards)
		{
			string? backgroundImageUrl = null;

			if (board.BackgroundImageObjectKey is { } objectKey && fileStorage.IsConfigured)
			{
				backgroundImageUrl = await fileStorage.CreatePresignedDownloadUrlAsync(objectKey, TimeSpan.FromHours(1), cancellationToken);
			}

			dtos.Add(new AnalysisBoardDto(
				board.Id, board.MapName.ToString(), board.Title, board.BackgroundImageObjectKey, backgroundImageUrl, board.StrokesJson, board.UpdatedAtUtc));
		}

		return dtos;
	}

	#endregion
}
