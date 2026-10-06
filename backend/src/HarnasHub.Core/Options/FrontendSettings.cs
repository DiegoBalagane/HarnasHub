namespace HarnasHub.Core.Options;

/// <summary>Where the web app lives, used to put links to it into Discord messages.</summary>
public class FrontendSettings
{
	public const string SectionName = "Frontend";

	/// <summary>Public origin of the frontend, e.g. "https://harnashub.example.com". Empty omits links from messages.</summary>
	public string BaseUrl { get; set; } = string.Empty;
}
