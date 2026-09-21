using HarnasHub.Application.Features.Results.PresignDemoUpload;
using HarnasHub.Tests.Common;
using Xunit;

namespace HarnasHub.Tests.Application.Features.Results.PresignDemoUpload;

public class PresignDemoUploadHandlerTests
{
	#region Public Methods

	[Fact]
	public async Task Should_return_a_presigned_upload_when_storage_is_configured()
	{
		var handler = new PresignDemoUploadHandler(new TestFileStorage(isConfigured: true));

		var result = await handler.Handle(new PresignDemoUploadCommand(), CancellationToken.None);

		Assert.False(result.IsError);
		Assert.False(string.IsNullOrWhiteSpace(result.Value.UploadUrl));
		Assert.False(string.IsNullOrWhiteSpace(result.Value.ObjectKey));
	}

	[Fact]
	public async Task Should_fail_when_storage_is_not_configured()
	{
		var handler = new PresignDemoUploadHandler(new TestFileStorage(isConfigured: false));

		var result = await handler.Handle(new PresignDemoUploadCommand(), CancellationToken.None);

		Assert.True(result.IsError);
		Assert.Equal("Results.StorageNotConfigured", result.FirstError.Code);
	}

	#endregion
}
