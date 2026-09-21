namespace HarnasHub.Core.Options;

/// <summary>Configuration for an S3-compatible object store (e.g. Cloudflare R2), used only for files too large to
/// comfortably pass through this app's own HTTP pipeline. Optional — leaving any of these blank simply disables the
/// large-file-upload feature rather than preventing the app from starting.</summary>
public class S3Settings
{
	public const string SectionName = "S3";

	/// <summary>The S3-compatible endpoint, e.g. https://&lt;account-id&gt;.r2.cloudflarestorage.com for Cloudflare R2.</summary>
	public string Endpoint { get; set; } = string.Empty;
	public string AccessKey { get; set; } = string.Empty;
	public string SecretKey { get; set; } = string.Empty;
	public string BucketName { get; set; } = string.Empty;
}
