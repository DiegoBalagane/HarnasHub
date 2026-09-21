using ErrorOr;
using HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Tests.Common;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.AnalyzeDemoFromStorage;

public class AnalyzeDemoFromStorageHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_delegate_to_the_regular_analyze_flow_and_delete_the_object_afterwards()
	{
		var expected = new AnalyzeDemoResultDto(
			24, "Mirage",
			new DemoTeamPreviewDto(["a"], ["1"], 16, 10),
			new DemoTeamPreviewDto(["b"], ["2"], 10, 16),
			null, []);
		var storage = new TestFileStorage(isConfigured: true, streamToReturn: new MemoryStream());
		var handler = new AnalyzeDemoFromStorageHandler(
			storage, new TestSender<ErrorOr<AnalyzeDemoResultDto>>(expected), NullLogger<AnalyzeDemoFromStorageHandler>.Instance);

		var result = await handler.Handle(new AnalyzeDemoFromStorageCommand("demos/abc"), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.Equal("Mirage", result.Value.MapName);
		Assert.Equal(["demos/abc"], storage.DeletedKeys);
	}

	[Fact]
	public async Task Should_fail_when_storage_is_not_configured()
	{
		var storage = new TestFileStorage(isConfigured: false);
		var handler = new AnalyzeDemoFromStorageHandler(
			storage, new TestSender<ErrorOr<AnalyzeDemoResultDto>>(default!), NullLogger<AnalyzeDemoFromStorageHandler>.Instance);

		var result = await handler.Handle(new AnalyzeDemoFromStorageCommand("demos/abc"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.StorageNotConfigured", result.FirstError.Code);
	}

	[Fact]
	public async Task Should_still_delete_the_object_when_opening_it_fails()
	{
		var storage = new TestFileStorage(isConfigured: true, throwOnOpenRead: new InvalidOperationException("gone"));
		var handler = new AnalyzeDemoFromStorageHandler(
			storage, new TestSender<ErrorOr<AnalyzeDemoResultDto>>(default!), NullLogger<AnalyzeDemoFromStorageHandler>.Instance);

		var result = await handler.Handle(new AnalyzeDemoFromStorageCommand("demos/abc"), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.InvalidDemoFile", result.FirstError.Code);
		Assert.Equal(["demos/abc"], storage.DeletedKeys);
	}

	#endregion
}
