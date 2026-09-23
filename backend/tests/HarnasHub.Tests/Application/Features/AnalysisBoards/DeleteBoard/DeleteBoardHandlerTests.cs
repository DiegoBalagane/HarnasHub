using HarnasHub.Application.Features.AnalysisBoards.DeleteBoard;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.AnalysisBoards.DeleteBoard;

public class DeleteBoardHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_board_and_its_background_image()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var board = new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Title = "Do usunięcia",
			BackgroundImageObjectKey = "analysis-boards/gone",
			StrokesJson = "[]",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		};
		dbContext.AnalysisBoards.Add(board);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var fileStorage = new TestFileStorage(isConfigured: true);
		var handler = new DeleteBoardHandler(dbContext, fileStorage, new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteBoardCommand(board.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.AnalysisBoards.ToListAsync());
		Assert.Contains("analysis-boards/gone", fileStorage.DeletedKeys);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_board()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new DeleteBoardHandler(dbContext, new TestFileStorage(), new TestRealtimeNotifier());

		var result = await handler.Handle(new DeleteBoardCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("AnalysisBoards.BoardNotFound", result.FirstError.Code);
	}

	#endregion
}
