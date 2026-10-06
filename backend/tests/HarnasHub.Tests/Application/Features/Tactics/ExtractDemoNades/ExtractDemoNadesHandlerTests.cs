#region Usings

using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Tactics.ExtractDemoNades;
using HarnasHub.Core.Enums;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using static HarnasHub.Tests.Application.Features.Tactics.DemoTimelineFactory;

#endregion

namespace HarnasHub.Tests.Application.Features.Tactics.ExtractDemoNades;

/// <summary>Covers the extract flow: parsing with only round+grenade collectors, map checks and guaranteed cleanup.</summary>
public class ExtractDemoNadesHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_rounds_with_grenades_and_delete_the_object()
	{
		var timeline = Timeline([Round(1, MapSide.T)], [Grenade(1, 1)]);
		var parser = new TestDemoParser(timeline: timeline);
		var storage = new TestFileStorage(isConfigured: true, streamToReturn: new MemoryStream([1, 2, 3]));
		var handler = new ExtractDemoNadesHandler(storage, parser, NullLogger<ExtractDemoNadesHandler>.Instance);

		var result = await handler.Handle(new ExtractDemoNadesCommand("demos/abc"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal(MapName.Mirage, result.Value.MapName);
		Assert.Single(Assert.Single(result.Value.Rounds).Grenades);
		Assert.Equal(["demos/abc"], storage.DeletedKeys);
		Assert.Equal(DemoParseOptions.RoundsAndGrenades, parser.LastOptions);
	}

	[Fact]
	public async Task Should_fail_when_the_map_is_not_calibrated()
	{
		var timeline = new DemoTimeline("de_vertigo", null, false, null, [], []);
		var storage = new TestFileStorage(isConfigured: true, streamToReturn: new MemoryStream());
		var handler = new ExtractDemoNadesHandler(storage, new TestDemoParser(timeline: timeline), NullLogger<ExtractDemoNadesHandler>.Instance);

		var result = await handler.Handle(new ExtractDemoNadesCommand("demos/abc"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Tactics.DemoMapNotSupported", result.FirstError.Code);
		Assert.Equal(["demos/abc"], storage.DeletedKeys);
	}

	[Fact]
	public async Task Should_fail_when_storage_is_not_configured()
	{
		var handler = new ExtractDemoNadesHandler(
			new TestFileStorage(isConfigured: false), new TestDemoParser(), NullLogger<ExtractDemoNadesHandler>.Instance);

		var result = await handler.Handle(new ExtractDemoNadesCommand("demos/abc"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.StorageNotConfigured", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_still_delete_the_object_when_parsing_fails()
	{
		var storage = new TestFileStorage(isConfigured: true, streamToReturn: new MemoryStream());
		var parser = new TestDemoParser(throwOnParse: new InvalidDataException("not a demo"));
		var handler = new ExtractDemoNadesHandler(storage, parser, NullLogger<ExtractDemoNadesHandler>.Instance);

		var result = await handler.Handle(new ExtractDemoNadesCommand("demos/abc"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.InvalidDemoFile", result.FirstError.Code);
		Assert.Equal(["demos/abc"], storage.DeletedKeys);
	}

	#endregion
}
