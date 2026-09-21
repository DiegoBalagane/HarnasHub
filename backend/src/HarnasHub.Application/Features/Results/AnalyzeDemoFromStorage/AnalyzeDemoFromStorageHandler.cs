using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.Results.AnalyzeDemo;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.Results.AnalyzeDemoFromStorage;

/// <summary>Handles <see cref="AnalyzeDemoFromStorageCommand"/> by streaming the object back from storage and
/// delegating the actual parsing to <see cref="AnalyzeDemoHandler"/> (via <see cref="AnalyzeDemoCommand"/>) so the
/// two upload paths can never drift on parsing behaviour. The object is deleted afterwards either way — nothing
/// uploaded through this feature is meant to be retained, successful analysis or not.</summary>
public class AnalyzeDemoFromStorageHandler(IFileStorage fileStorage, ISender sender, ILogger<AnalyzeDemoFromStorageHandler> logger)
	: IRequestHandler<AnalyzeDemoFromStorageCommand, ErrorOr<AnalyzeDemoResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<AnalyzeDemoResultDto>> Handle(AnalyzeDemoFromStorageCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return ResultErrors.StorageNotConfigured;
		}

		var tempFilePath = Path.GetTempFileName();

		try
		{
			// The demo parser needs to seek within the stream (it isn't a pure forward read), but an S3/R2 object's
			// response stream is a live, forward-only network stream — handing it straight to the parser fails
			// immediately with NotSupportedException. Buffering to a local temp file first gives it something seekable,
			// the same way ASP.NET Core's own IFormFile already does for a large multipart upload under the direct path.
			await using (var demoStream = await fileStorage.OpenReadAsync(request.ObjectKey, cancellationToken))
			await using (var tempFile = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await demoStream.CopyToAsync(tempFile, cancellationToken);
			}

			await using var seekableStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			return await sender.Send(new AnalyzeDemoCommand(seekableStream), cancellationToken);
		}
		catch (Exception ex)
		{
			// The object might be missing/expired, or the bucket briefly unreachable — either way this is a
			// validation-shaped problem for the caller (ask them to upload again), not a server bug to surface raw.
			// Logged in full here since the client only ever sees a generic "couldn't read this as a CS2 demo" message.
			logger.LogWarning(ex, "Nie udało się odczytać demki z magazynu (ObjectKey={ObjectKey})", request.ObjectKey);
			return ResultErrors.InvalidDemoFile;
		}
		finally
		{
			try
			{
				File.Delete(tempFilePath);
			}
			catch (Exception)
			{
				// Best-effort — a stray temp file in a container that gets recycled on every deploy is harmless.
			}

			try
			{
				await fileStorage.DeleteAsync(request.ObjectKey, cancellationToken);
			}
			catch (Exception)
			{
				// Best-effort cleanup — an R2 bucket lifecycle rule is the real backstop for anything this misses.
			}
		}
	}

	#endregion
}
