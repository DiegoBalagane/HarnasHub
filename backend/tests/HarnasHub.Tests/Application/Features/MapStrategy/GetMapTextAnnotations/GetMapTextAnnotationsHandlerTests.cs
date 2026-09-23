using HarnasHub.Application.Features.MapStrategy.GetMapTextAnnotations;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.MapStrategy.GetMapTextAnnotations;

public class GetMapTextAnnotationsHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_only_return_annotations_for_the_requested_map_and_side()
	{
		await using var dbContext = TestApplicationDbContext.Create();
		dbContext.MapTextAnnotations.Add(new MapTextAnnotation
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Side = MapSide.CT,
			Text = "Widoczna",
			Color = "#ffffff",
			FontSizePx = 14,
			X = 0.2f,
			Y = 0.2f,
			CreatedByUserId = Guid.NewGuid(),
			UpdatedAtUtc = DateTime.UtcNow
		});
		dbContext.MapTextAnnotations.Add(new MapTextAnnotation
		{
			Id = Guid.NewGuid(),
			MapName = MapName.Mirage,
			Side = MapSide.T,
			Text = "Inna strona",
			Color = "#ffffff",
			FontSizePx = 14,
			X = 0.2f,
			Y = 0.2f,
			CreatedByUserId = Guid.NewGuid(),
			UpdatedAtUtc = DateTime.UtcNow
		});
		await dbContext.SaveChangesAsync(CancellationToken.None);

		var handler = new GetMapTextAnnotationsHandler(dbContext);

		var result = await handler.Handle(new GetMapTextAnnotationsQuery(MapName.Mirage, MapSide.CT), CancellationToken.None);

		Assert.False(result.IsError);
		var annotation = Assert.Single(result.Value);
		Assert.Equal("Widoczna", annotation.Text);
	}

	#endregion
}
