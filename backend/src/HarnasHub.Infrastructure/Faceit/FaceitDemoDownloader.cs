#region Usings

using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HarnasHub.Application.Abstractions;
using HarnasHub.Core.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZstdSharp;

#endregion

namespace HarnasHub.Infrastructure.Faceit;

/// <summary>Typed <see cref="HttpClient"/> over the FACEIT Downloads API (docs.faceit.com → Guides → Download API):
/// <c>POST {DownloadsBaseUrl}demos/download</c> with <c>{"resource_url": …}</c> and the Downloads-scoped bearer token returns
/// <c>{"payload":{"download_url": …}}</c>, a signed URL fetched without auth. CS2 demos come zstd-compressed (.dem.zst),
/// older ones gzip (.dem.gz); the format is recognised by its magic bytes, so a renamed/plain .dem works too.</summary>
public class FaceitDemoDownloader(HttpClient httpClient, IOptions<FaceitOptions> options, ILogger<FaceitDemoDownloader> logger)
	: IFaceitDemoDownloader
{
	#region Private Fields

	private static readonly byte[] ZstdMagic = [0x28, 0xB5, 0x2F, 0xFD];
	private static readonly byte[] GzipMagic = [0x1F, 0x8B];

	#endregion

	#region Public Properties

	/// <inheritdoc />
	public bool IsConfigured => !string.IsNullOrWhiteSpace(options.Value.DownloadsApiToken);

	#endregion

	#region Public Methods

	/// <inheritdoc />
	public async Task DownloadAsync(string demoResourceUrl, string destinationPath, CancellationToken cancellationToken)
	{
		if (!IsConfigured)
		{
			throw new InvalidOperationException("Brak tokenu FACEIT Downloads API (Faceit:DownloadsApiToken).");
		}

		var signedUrl = await GetSignedUrlAsync(demoResourceUrl, cancellationToken);
		var compressedPath = destinationPath + ".download";

		try
		{
			using (var response = await httpClient.GetAsync(signedUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
			{
				response.EnsureSuccessStatusCode();
				await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
				await using var file = new FileStream(compressedPath, FileMode.Create, FileAccess.Write, FileShare.None);
				await body.CopyToAsync(file, cancellationToken);
			}

			await DecompressAsync(compressedPath, destinationPath, cancellationToken);
		}
		finally
		{
			File.Delete(compressedPath);
		}
	}

	#endregion

	#region Private Methods

	private async Task<string> GetSignedUrlAsync(string resourceUrl, CancellationToken cancellationToken)
	{
		var uri = new Uri(new Uri(EnsureTrailingSlash(options.Value.DownloadsBaseUrl)), "demos/download");
		using var request = new HttpRequestMessage(HttpMethod.Post, uri)
		{
			Content = JsonContent.Create(new Dictionary<string, string> { ["resource_url"] = resourceUrl })
		};
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.DownloadsApiToken);

		using var response = await httpClient.SendAsync(request, cancellationToken);
		if (!response.IsSuccessStatusCode)
		{
			logger.LogWarning("FACEIT Downloads API zwróciło {StatusCode} dla {ResourceUrl}", (int)response.StatusCode, resourceUrl);
			throw new HttpRequestException($"FACEIT Downloads API zwróciło {(int)response.StatusCode}.", null, response.StatusCode);
		}

		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
		var url = document.RootElement.TryGetProperty("payload", out var payload)
			&& payload.TryGetProperty("download_url", out var downloadUrl)
				? downloadUrl.GetString()
				: null;

		return string.IsNullOrWhiteSpace(url)
			? throw new InvalidDataException("FACEIT Downloads API nie zwróciło adresu pobrania.")
			: url;
	}

	/// <summary>Writes the plain .dem to <paramref name="destinationPath"/>, unwrapping zstd or gzip when the file starts with
	/// their magic bytes and copying it as-is otherwise.</summary>
	private static async Task DecompressAsync(string sourcePath, string destinationPath, CancellationToken cancellationToken)
	{
		await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
		var header = new byte[4];
		var read = await source.ReadAsync(header, cancellationToken);
		source.Position = 0;

		await using var destination = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
		if (read >= 4 && header.AsSpan(0, 4).SequenceEqual(ZstdMagic))
		{
			await using var zstd = new DecompressionStream(source);
			await zstd.CopyToAsync(destination, cancellationToken);
		}
		else if (read >= 2 && header.AsSpan(0, 2).SequenceEqual(GzipMagic))
		{
			await using var gzip = new GZipStream(source, CompressionMode.Decompress);
			await gzip.CopyToAsync(destination, cancellationToken);
		}
		else
		{
			await source.CopyToAsync(destination, cancellationToken);
		}
	}

	private static string EnsureTrailingSlash(string url) => url.EndsWith('/') ? url : url + "/";

	#endregion
}
