#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IFileStorage"/> — configurable presence/behaviour instead of talking to real object storage.
/// Uploaded objects are kept in memory, so anything written can be read back by key.</summary>
public class TestFileStorage(bool isConfigured = true, Stream? streamToReturn = null, Exception? throwOnOpenRead = null) : IFileStorage
{
	#region Public Properties

	/// <inheritdoc />
	public bool IsConfigured { get; } = isConfigured;

	/// <summary>Every key passed to <see cref="DeleteAsync"/>, in order.</summary>
	public List<string> DeletedKeys { get; } = [];

	/// <summary>Bytes of every object uploaded (and not deleted), by key.</summary>
	public Dictionary<string, byte[]> Objects { get; } = [];

	/// <summary>When set, <see cref="UploadAsync"/> throws it instead of storing anything.</summary>
	public Exception? ThrowOnUpload { get; set; }

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public Task<PresignedUpload> CreatePresignedUploadAsync(string keyPrefix, TimeSpan expiry, CancellationToken cancellationToken) =>
		Task.FromResult(new PresignedUpload($"https://example.com/upload/{keyPrefix}", $"{keyPrefix}/{Guid.NewGuid():N}"));

	/// <inheritdoc />
	public Task<string> CreatePresignedDownloadUrlAsync(string objectKey, TimeSpan expiry, CancellationToken cancellationToken) =>
		Task.FromResult($"https://example.com/download/{objectKey}");

	/// <inheritdoc />
	public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
	{
		if (throwOnOpenRead is not null)
		{
			throw throwOnOpenRead;
		}

		if (Objects.TryGetValue(objectKey, out var bytes))
		{
			return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
		}

		return Task.FromResult(streamToReturn ?? Stream.Null);
	}

	/// <inheritdoc />
	public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
	{
		DeletedKeys.Add(objectKey);
		Objects.Remove(objectKey);
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public async Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken)
	{
		if (ThrowOnUpload is not null)
		{
			throw ThrowOnUpload;
		}

		using var buffer = new MemoryStream();
		await content.CopyToAsync(buffer, cancellationToken);
		Objects[objectKey] = buffer.ToArray();
	}

	/// <inheritdoc />
	public Task<int> DeleteOlderThanAsync(string keyPrefix, DateTime olderThanUtc, CancellationToken cancellationToken)
	{
		var keys = Objects.Keys.Where(k => k.StartsWith(keyPrefix, StringComparison.Ordinal)).ToList();
		foreach (var key in keys)
		{
			Objects.Remove(key);
		}

		return Task.FromResult(keys.Count);
	}

	#endregion
}
