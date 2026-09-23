using HarnasHub.Application.Features.AnalysisBoards.GetBoards;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.AnalysisBoards.GetBoards;

public class GetBoardsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_only_return_boards_for_the_requested_map()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.AnalysisBoards.Add(new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Title = "Mirage board",
			StrokesJson = "[]",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		});
		dbContext.AnalysisBoards.Add(new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Ancient,
			Title = "Ancient board",
			StrokesJson = "[]",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetBoardsHandler(dbContext, new TestFileStorage());

		var result = await handler.Handle(new GetBoardsQuery(MapName.Mirage), CancellationToken.None);

		Assert.False(result.IsError);
		var board = Assert.Single(result.Value);
		Assert.Equal("Mirage board", board.Title);
	}

	[Fact]
	public async Task Should_leave_the_background_url_null_when_storage_is_not_configured()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.AnalysisBoards.Add(new AnalysisBoard
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Title = "Screen",
			BackgroundImageObjectKey = "analysis-boards/xyz",
			StrokesJson = "[]",
			CreatedByUserId = Guid.NewGuid(),
			CreatedAtUtc = DateTime.UtcNow,
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetBoardsHandler(dbContext, new TestFileStorage(isConfigured: false));

		var result = await handler.Handle(new GetBoardsQuery(null), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Null(Assert.Single(result.Value).BackgroundImageUrl);
	}

	#endregion
}
