#region Usings

using HarnasHub.Application.Abstractions;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.Shared;

/// <summary>Reads an opponent demo into a timeline with position sampling on. The parser needs a seekable stream, so an
/// uploaded object is first buffered to a temp file; the uploaded .dem is deleted from storage whatever happens.</summary>
public static class OpponentDemoReader
{
	#region Public Methods

	/// <summary>Parses the uploaded object <paramref name="objectKey"/>; null when it couldn't be read as a demo.</summary>
	public static async Task<DemoTimeline?> ParseUploadedAsync(
		IFileStorage fileStorage, IDemoParser parser, string objectKey, ILogger logger, CancellationToken cancellationToken)
	{
		var tempFilePath = Path.GetTempFileName();

		try
		{
			await using (var demoStream = await fileStorage.OpenReadAsync(objectKey, cancellationToken))
			await using (var tempFile = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await demoStream.CopyToAsync(tempFile, cancellationToken);
			}

			return await ParseFileAsync(parser, tempFilePath, logger, cancellationToken);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się odczytać demki przeciwnika z magazynu (ObjectKey={ObjectKey})", objectKey);
			return null;
		}
		finally
		{
			TryDeleteFile(tempFilePath);

			try
			{
				// Not tied to the request's token: a cancelled request must still not leave the demo in the bucket.
				await fileStorage.DeleteAsync(objectKey, CancellationToken.None);
			}
			catch (Exception)
			{
				// Best-effort — the bucket's lifecycle rule is the backstop.
			}
		}
	}

	/// <summary>Parses a local .dem file; null (logged) when the parser rejects it or it has no official rounds.</summary>
	public static async Task<DemoTimeline?> ParseFileAsync(IDemoParser parser, string path, ILogger logger, CancellationToken cancellationToken)
	{
		try
		{
			await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
			var timeline = await parser.ParseAsync(stream, DemoParseOptions.OpponentAnalysis, cancellationToken);
			return timeline.Rounds.Count == 0 ? null : timeline;
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się sparsować demki przeciwnika");
			return null;
		}
	}

	/// <summary>Deletes a local temp file, ignoring failures (a stray temp file in a recycled container is harmless).</summary>
	public static void TryDeleteFile(string path)
	{
		try
		{
			File.Delete(path);
		}
		catch (Exception)
		{
			// Best-effort.
		}
	}

	#endregion
}
