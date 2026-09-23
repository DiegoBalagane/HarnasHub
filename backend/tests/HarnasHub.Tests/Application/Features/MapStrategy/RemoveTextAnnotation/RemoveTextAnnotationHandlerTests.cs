using HarnasHub.Application.Features.MapStrategy.RemoveTextAnnotation;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.RemoveTextAnnotation;

public class RemoveTextAnnotationHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delete_an_existing_annotation()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var annotation = new MapTextAnnotation
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Side = MapSide.CT,
			Text = "Do usunięcia",
			Color = "#ffffff",
			FontSizePx = 14,
			X = 0.2f,
			Y = 0.2f,
			CreatedByUserId = Guid.NewGuid(),
			UpdatedAtUtc = DateTime.UtcNow
		};
		dbContext.MapTextAnnotations.Add(annotation);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new RemoveTextAnnotationHandler(dbContext, notifier);

		var result = await handler.Handle(new RemoveTextAnnotationCommand(annotation.Id), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Empty(await dbContext.MapTextAnnotations.ToListAsync());
		Assert.Equal(["map-strategy"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_annotation()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new RemoveTextAnnotationHandler(dbContext, new TestRealtimeNotifier());

		var result = await handler.Handle(new RemoveTextAnnotationCommand(Guid.NewGuid()), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("MapStrategy.AnnotationNotFound", result.FirstError.Code);
	}

	#endregion
}
