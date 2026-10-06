namespace HarnasHub.Application.Abstractions;

/// <summary>FACEIT Downloads API: turns a match's demo resource URL (<c>demo_url</c> of the Data API match) into a signed
/// URL and downloads the demo, decompressed to a plain .dem. Needs its own access token with the Downloads API scope,
/// granted by FACEIT on application — without it <see cref="IsConfigured"/> is false and demos are uploaded by hand.</summary>
public interface IFaceitDemoDownloader
{
	/// <summary>Whether a Downloads API token is configured.</summary>
	bool IsConfigured { get; }

	/// <summary>Downloads the demo behind <paramref name="demoResourceUrl"/> to <paramref name="destinationPath"/> as an
	/// uncompressed .dem; throws <see cref="HttpRequestException"/>/<see cref="InvalidDataException"/> on failure.</summary>
	Task DownloadAsync(string demoResourceUrl, string destinationPath, CancellationToken cancellationToken);
}
