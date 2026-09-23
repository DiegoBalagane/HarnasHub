using HarnasHub.Application.Abstractions;

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IFileStorage"/> — configurable presence/behaviour instead of talking to real object storage.</summary>
public class TestFileStorage(bool isConfigured = true, Stream? streamToReturn = null, Exception? throwOnOpenRead = null) : IFileStorage
{
	#region Public Properties

	public bool IsConfigured { get; } = isConfigured;
	public List<string> DeletedKeys { get; } = [];

	#endregion

	#region Public Methods

	public Task<PresignedUpload> CreatePresignedUploadAsync(string keyPrefix, TimeSpan expiry, CancellationToken cancellationToken) =>
		Task.FromResult(new PresignedUpload($"https://example.com/upload/{keyPrefix}", $"{keyPrefix}/{Guid.NewGuid():N}"));

	public Task<string> CreatePresignedDownloadUrlAsync(string objectKey, TimeSpan expiry, CancellationToken cancellationToken) =>
		Task.FromResult($"https://example.com/download/{objectKey}");

	public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
	{
		if (throwOnOpenRead is not null)
		{
			throw throwOnOpenRead;
		}

		return Task.FromResult(streamToReturn ?? Stream.Null);
	}

	public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
	{
		DeletedKeys.Add(objectKey);
		return Task.CompletedTask;
	}

	#endregion
}
