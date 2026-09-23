using HarnasHub.Application.Features.MapStrategy.UpdateTextAnnotation;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.UpdateTextAnnotation;

public class UpdateTextAnnotationHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_update_text_color_size_and_position()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var annotation = new MapTextAnnotation
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Side = MapSide.CT,
			Text = "Stara treść",
			Color = "#ffffff",
			FontSizePx = 14,
			X = 0.2f,
			Y = 0.2f,
			CreatedByUserId = Guid.NewGuid(),
			UpdatedAtUtc = DateTime.UtcNow.AddDays(-1)
		};
		dbContext.MapTextAnnotations.Add(annotation);
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var notifier = new TestRealtimeNotifier();
		var handler = new UpdateTextAnnotationHandler(dbContext, notifier);

		var command = new UpdateTextAnnotationCommand(annotation.Id, "Nowa treść", "#00ff00", 24, 0.6f, 0.7f);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		var stored = await dbContext.MapTextAnnotations.SingleAsync();
		Assert.Equal("Nowa treść", stored.Text);
		Assert.Equal("#00ff00", stored.Color);
		Assert.Equal(24, stored.FontSizePx);
		Assert.Equal(0.6f, stored.X);
		Assert.Equal(0.7f, stored.Y);
		Assert.Equal(["map-strategy"], notifier.Topics);
	}

	[Fact]
	public async Task Should_return_not_found_for_an_unknown_annotation()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var handler = new UpdateTextAnnotationHandler(dbContext, new TestRealtimeNotifier());

		var command = new UpdateTextAnnotationCommand(Guid.NewGuid(), "Treść", "#ffffff", 14, 0.5f, 0.5f);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("MapStrategy.AnnotationNotFound", result.FirstError.Code);
	}

	#endregion
}
