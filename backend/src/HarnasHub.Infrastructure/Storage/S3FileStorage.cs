using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Options;

namespace HarnasHub.Infrastructure.Storage;

/// <summary>Implements <see cref="IFileStorage"/> against any S3-compatible provider (built and tested against
/// Cloudflare R2) via the AWS SDK — R2 speaks the same API, just with its own endpoint and no real AWS region.</summary>
public class S3FileStorage : IFileStorage
{
	#region Private Fields

	private readonly S3Settings _settings;
	private readonly Lazy<AmazonS3Client> _client;

	#endregion

	#region Constructors

	public S3FileStorage(IOptions<S3Settings> settings)
	{
		_settings = settings.Value;

		// Built lazily so an unconfigured deployment (IsConfigured false) never even constructs a client
		// pointed at an empty endpoint.
		_client = new Lazy<AmazonS3Client>(() => new AmazonS3Client(
			new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey),
			new AmazonS3Config
			{
				ServiceURL = _settings.Endpoint,
				// R2 (and most non-AWS S3-compatible providers) only understands path-style requests
				// (endpoint/bucket/key), not AWS's virtual-hosted-style (bucket.endpoint/key).
				ForcePathStyle = true,
				AuthenticationRegion = "auto"
			}));
	}

	#endregion

	#region Private Properties

	// GetPreSignedURL signs https:// links regardless of ServiceURL, which breaks a plain-http endpoint such as the local
	// docker-compose S3 — follow whatever scheme the configured endpoint actually uses.
	private Protocol PresignProtocol =>
		_settings.Endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ? Protocol.HTTP : Protocol.HTTPS;

	#endregion

	#region Public Properties

	public bool IsConfigured =>
		!string.IsNullOrWhiteSpace(_settings.Endpoint)
		&& !string.IsNullOrWhiteSpace(_settings.AccessKey)
		&& !string.IsNullOrWhiteSpace(_settings.SecretKey)
		&& !string.IsNullOrWhiteSpace(_settings.BucketName);

	#endregion

	#region Public Methods

	public Task<PresignedUpload> CreatePresignedUploadAsync(string keyPrefix, TimeSpan expiry, CancellationToken cancellationToken)
	{
		var objectKey = $"{keyPrefix}/{Guid.NewGuid():N}";

		var request = new GetPreSignedUrlRequest
		{
			BucketName = _settings.BucketName,
			Key = objectKey,
			Verb = HttpVerb.PUT,
			Protocol = PresignProtocol,
			Expires = DateTime.UtcNow.Add(expiry),
			// Left unset deliberately: the browser's PUT must not send a Content-Type the signature didn't
			// account for, and demo files have no single canonical MIME type worth pinning here anyway.
		};

		var url = _client.Value.GetPreSignedURL(request);
		return Task.FromResult(new PresignedUpload(url, objectKey));
	}

	public Task<string> CreatePresignedDownloadUrlAsync(string objectKey, TimeSpan expiry, CancellationToken cancellationToken)
	{
		var request = new GetPreSignedUrlRequest
		{
			BucketName = _settings.BucketName,
			Key = objectKey,
			Verb = HttpVerb.GET,
			Protocol = PresignProtocol,
			Expires = DateTime.UtcNow.Add(expiry)
		};

		return Task.FromResult(_client.Value.GetPreSignedURL(request));
	}

	public async Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
	{
		var response = await _client.Value.GetObjectAsync(_settings.BucketName, objectKey, cancellationToken);
		return response.ResponseStream;
	}

	public async Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
	{
		await _client.Value.DeleteObjectAsync(_settings.BucketName, objectKey, cancellationToken);
	}

	public async Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken cancellationToken)
	{
		var request = new PutObjectRequest
		{
			BucketName = _settings.BucketName,
			Key = objectKey,
			InputStream = content,
			ContentType = contentType,
			AutoCloseStream = false,
			// R2 rejects the AWS SDK's default streaming (chunked) signature payloads. The SDK only allows an unsigned
			// payload over HTTPS, so a plain-http endpoint (local docker-compose S3) signs the whole body instead.
			DisablePayloadSigning = PresignProtocol == Protocol.HTTPS,
			UseChunkEncoding = false
		};

		await _client.Value.PutObjectAsync(request, cancellationToken);
	}

	public async Task<int> DeleteOlderThanAsync(string keyPrefix, DateTime olderThanUtc, CancellationToken cancellationToken)
	{
		var deleted = 0;
		var request = new ListObjectsV2Request { BucketName = _settings.BucketName, Prefix = keyPrefix };

		ListObjectsV2Response response;
		do
		{
			response = await _client.Value.ListObjectsV2Async(request, cancellationToken);

			var stale = (response.S3Objects ?? [])
				.Where(o => o.LastModified is { } modified && modified.ToUniversalTime() < olderThanUtc)
				.ToList();

			foreach (var staleObject in stale)
			{
				await _client.Value.DeleteObjectAsync(_settings.BucketName, staleObject.Key, cancellationToken);
				deleted++;
			}

			request.ContinuationToken = response.NextContinuationToken;
		}
		while (response.IsTruncated == true);

		return deleted;
	}

	#endregion
}
