#region Usings

using HarnasHub.Application.Features.OpponentReport.DeleteOpponentDemo;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using static HarnasHub.Tests.Application.Features.OpponentReport.OpponentDemoRows;

#endregion

namespace HarnasHub.Tests.Application.Features.OpponentReport.DeleteOpponentDemo;

public class DeleteOpponentDemoHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_the_row_and_its_timeline()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var row = Row();
		dbContext.OpponentDemoAnalyses.Add(row);
		await dbContext.SaveChangesAsync(CancellationToken.None);
		var storage = new TestFileStorage();

		var result = await new DeleteOpponentDemoHandler(dbContext, storage, NullLogger<DeleteOpponentDemoHandler>.Instance)
			.Handle(new DeleteOpponentDemoCommand(row.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(dbContext.OpponentDemoAnalyses);
		Assert.Contains(row.TimelineObjectKey, storage.DeletedKeys);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_demo()
	{
		await using var dbContext = TestApplicationDbContext.Create();

		var result = await new DeleteOpponentDemoHandler(dbContext, new TestFileStorage(), NullLogger<DeleteOpponentDemoHandler>.Instance)
			.Handle(new DeleteOpponentDemoCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.Equal(OpponentDemoErrors.NotFound.Code, result.FirstError.Code);
	}

	[Fact]
	public void Should_require_an_id()
	{
		Assert.False(new DeleteOpponentDemoCommandValidator().Validate(new DeleteOpponentDemoCommand(Guid.Empty)).IsValid);
	}

	#endregion
}
