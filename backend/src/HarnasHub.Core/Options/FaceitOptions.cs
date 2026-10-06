namespace HarnasHub.Core.Options;

/// <summary>Configuration for the FACEIT Data API v4 integration; an empty <see cref="ApiKey"/> disables it without breaking startup.</summary>
public class FaceitOptions
{
	#region Public Fields

	/// <summary>Configuration section the options are bound from.</summary>
	public const string SectionName = "Faceit";

	#endregion

	#region Public Properties

	/// <summary>Server-side Data API key (FACEIT developer portal); keep it in user-secrets or an environment variable, never in the repo.</summary>
	public string ApiKey { get; set; } = string.Empty;

	/// <summary>Base address of the Data API, with a trailing slash.</summary>
	public string BaseUrl { get; set; } = "https://open.faceit.com/data/v4/";

	/// <summary>Access token with the Downloads API scope (granted by FACEIT on application); empty disables automatic
	/// demo downloads — opponent demos are then uploaded by hand. Keep it in user-secrets or an environment variable.</summary>
	public string DownloadsApiToken { get; set; } = string.Empty;

	/// <summary>Base address of the Downloads API, with a trailing slash.</summary>
	public string DownloadsBaseUrl { get; set; } = "https://open.faceit.com/download/v2/";

	/// <summary>How often the background sync refreshes opponents with an upcoming match and our own team.</summary>
	public int SyncIntervalHours { get; set; } = 6;

	/// <summary>How many days ahead an event must start for its opponent to be refreshed in the background.</summary>
	public int UpcomingEventDays { get; set; } = 7;

	#endregion
}
