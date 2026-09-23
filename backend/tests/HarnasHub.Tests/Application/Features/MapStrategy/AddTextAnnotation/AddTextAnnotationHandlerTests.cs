using HarnasHub.Application.Features.MapStrategy.AddTextAnnotation;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.AddTextAnnotation;

public class AddTextAnnotationHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_insert_an_annotation_and_notify()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		var notifier = new TestRealtimeNotifier();
		var handler = new AddTextAnnotationHandler(dbContext, new TestCurrentUserService(Guid.NewGuid()), notifier);

		var command = new AddTextAnnotationCommand(MapName.Mirage, MapSide.CT, "Tu smoke na window", "#ff0000", 16, 0.3f, 0.4f);

		var result = await handler.Handle(command, CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Tu smoke na window", result.Value.Text);
		Assert.Equal("#ff0000", result.Value.Color);
		Assert.Equal(16, result.Value.FontSizePx);

		var stored = await dbContext.MapTextAnnotations.SingleAsync();
		Assert.Equal(MapName.Mirage, stored.MapName);
		Assert.Equal(MapSide.CT, stored.Side);
		Assert.Equal(0.3f, stored.X);
		Assert.Equal(0.4f, stored.Y);
		Assert.Equal(["map-strategy"], notifier.Topics);
	}

	#endregion
}
