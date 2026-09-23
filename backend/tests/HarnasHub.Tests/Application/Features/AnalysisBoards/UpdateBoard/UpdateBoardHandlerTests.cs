using HarnasHub.Application.Features.AnalysisBoards.UpdateBoard;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.AnalysisBoards.UpdateBoard;

public class UpdateBoardHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_the_boards_content_and_delete_the_replaced_background()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var board = new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Title = "Stary tytuł",
			BackgroundImageObjectKey = "analysis-boards/old",
			StrokesJson = "[]",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
		};
		dbContext.AnalysisBoards.Add(board);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var fileStorage = new TestFileStorage(isConfigured: true);
		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateBoardHandler(dbContext, fileStorage, notifier);

		var command = new UpdateBoardCommand(board.Id, "Nowy tytuł", "analysis-boards/new", "[{\"points\":[]}]");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Nowy tytuł", result.Value.Title);
		Assert.Contains("analysis-boards/new", result.Value.BackgroundImageUrl);
		Assert.Contains("analysis-boards/old", fileStorage.DeletedKeys);
		Assert.Equal(["analysis-boards"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_a_missing_board()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateBoardHandler(dbContext, new TestFileStorage(), new TestRealtimeNotifier());

		var command = new UpdateBoardCommand(Guid.NewGuid(), "Tytuł", null, "[]");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("AnalysisBoards.BoardNotFound", result.FirstError.Code);
	}

	#endregion
}
