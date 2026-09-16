namespace HarnasHub.Core.Options;

/// <summary>Configuration for signing and validating JWT access tokens.</summary>
public class JwtSettings
{
	public const string SectionName = "Jwt";

	public string Secret { get; set; } = string.Empty;
	public string Issuer { get; set; } = string.Empty;
	public string Audience { get; set; } = string.Empty;
	/// <summary>Access token lifetime; defaults to 30 days so the team isn't forced through Discord sign-in every hour.</summary>
	public int ExpiryMinutes { get; set; } = 43200;
}
