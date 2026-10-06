#region Usings

using HarnasHub.Application.Abstractions;

#endregion

namespace HarnasHub.Tests.Common;

/// <summary>Stub <see cref="IFaceitDemoDownloader"/>: writes a few placeholder bytes instead of downloading (the fake parser
/// never reads them), records the requested URLs and can be told to fail for specific ones.</summary>
public class TestFaceitDemoDownloader(bool isConfigured = true) : IFaceitDemoDownloader
{
	#region Public Properties

	/// <inheritdoc />
	public bool IsConfigured { get; } = isConfigured;

	/// <summary>Every URL passed to <see cref="DownloadAsync"/>, in order.</summary>
	public List<string> Requested { get; } = [];

	/// <summary>URLs whose download throws.</summary>
	public HashSet<string> FailingUrls { get; } = [];

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task DownloadAsync(string demoResourceUrl, string destinationPath, CancellationToken cancellationToken)
	{
		Requested.Add(demoResourceUrl);
		if (FailingUrls.Contains(demoResourceUrl))
		{
			throw new HttpRequestException("download failed");
		}

		await File.WriteAllBytesAsync(destinationPath, [1, 2, 3], cancellationToken);
	}

	#endregion
}
