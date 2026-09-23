using HarnasHub.Application.Features.AnalysisBoards.CreateBoard;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.AnalysisBoards.CreateBoard;

public class CreateBoardHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_persist_a_board_drawn_over_the_built_in_radar()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new CreateBoardHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestFileStorage(), notifier);

		var command = new CreateBoardCommand(MapName.Mirage, "Setup vs Team X", null, "[{\"color\":\"#ff0000\"}]");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Setup vs Team X", result.Value.Title);
		Assert.Null(result.Value.BackgroundImageUrl);

		var stored = dbContext.AnalysisBoards.Single();
		Assert.Equal(MapName.Mirage, stored.MapName);
		Assert.Null(stored.BackgroundImageObjectKey);
		Assert.Equal(["analysis-boards"], notifier.Topics);
	}

	[Fact]
	public async Task Should_resolve_a_presigned_url_for_an_uploaded_background()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new CreateBoardHandler(
			dbContext, new TestCurrentUserService(Guid.NewGuid()), new TestFileStorage(isConfigured: true), new TestRealtimeNotifier());

		var command = new CreateBoardCommand(MapName.Ancient, "Screen z demki", "analysis-boards/abc123", "[]");
		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.NotNull(result.Value.BackgroundImageUrl);
		Assert.Contains("analysis-boards/abc123", result.Value.BackgroundImageUrl);
	}

	#endregion
}
