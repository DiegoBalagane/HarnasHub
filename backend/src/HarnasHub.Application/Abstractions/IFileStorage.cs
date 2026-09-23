namespace HarnasHub.Application.Abstractions;

/// <summary>A presigned upload target — the caller (browser) PUTs the file straight to <paramref name="UploadUrl"/>,
/// never through this app's own server, so an S3-compatible provider's own (much higher) size limit applies instead
/// of whatever the hosting platform's edge proxy allows through in one request.</summary>
public record PresignedUpload(string UploadUrl, string ObjectKey);

/// <summary>Abstraction over an S3-compatible object store (see HarnasHub.Infrastructure for the implementation) —
/// used for files too large to comfortably pass through this app's own HTTP pipeline, like CS2 demos, and for
/// retained user content like an analysis board's uploaded background screenshot. Never used for anything that
/// needs to be queried or joined against; it's strictly blob in, blob out.</summary>
public interface IFileStorage
{
	#region Public Properties

	/// <summary>False when the required configuration (endpoint/credentials/bucket) isn't set — callers should
	/// degrade to a smaller-file-only path rather than the app failing to start over an optional feature.</summary>
	bool IsConfigured { get; }

	#endregion

	#region Public Methods

	/// <summary>Creates a time-limited URL the browser can PUT a file to directly, under a fresh key prefixed with
	/// <paramref name="keyPrefix"/>.</summary>
	Task<PresignedUpload> CreatePresignedUploadAsync(string keyPrefix, TimeSpan expiry, CancellationToken cancellationToken);

	/// <summary>Creates a time-limited URL the browser can GET a previously uploaded, retained object from directly —
	/// for content like an analysis board's background image, which unlike a demo is meant to be viewed again later.</summary>
	Task<string> CreatePresignedDownloadUrlAsync(string objectKey, TimeSpan expiry, CancellationToken cancellationToken);

	/// <summary>Opens a previously uploaded object for reading — the caller owns disposing the stream.</summary>
	Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken);

	/// <summary>Deletes an object — used right after it's been read for a transient one (e.g. a demo), or when a
	/// retained one (e.g. an analysis board's background) is no longer referenced by anything.</summary>
	Task DeleteAsync(string objectKey, CancellationToken cancellationToken);

	#endregion
}
