#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Maps;
using HarnasHub.Application.Features.Results.Shared;
using HarnasHub.Application.Features.Tactics.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.Tactics.ExtractDemoNades;

/// <summary>Handles <see cref="ExtractDemoNadesCommand"/>: buffers the stored demo to a temp file (the parser needs a
/// seekable stream, see <c>AnalyzeDemoFromStorageHandler</c>), parses it with only the round and grenade collectors,
/// and always removes both the temp file and the stored object.</summary>
public class ExtractDemoNadesHandler(IFileStorage fileStorage, IDemoParser demoParser, ILogger<ExtractDemoNadesHandler> logger)
	: IRequestHandler<ExtractDemoNadesCommand, ErrorOr<DemoNadesDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<DemoNadesDto>> Handle(ExtractDemoNadesCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return ResultErrors.StorageNotConfigured;
		}

		DemoTimeline timeline;
		var tempFilePath = Path.GetTempFileName();

		try
		{
			await using (var demoStream = await fileStorage.OpenReadAsync(request.ObjectKey, cancellationToken))
			await using (var tempFile = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await demoStream.CopyToAsync(tempFile, cancellationToken);
			}

			await using var seekableStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			timeline = await demoParser.ParseAsync(seekableStream, DemoParseOptions.RoundsAndGrenades, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Nie udało się odczytać granatów z demki (ObjectKey={ObjectKey})", request.ObjectKey);
			return ResultErrors.InvalidDemoFile;
		}
		finally
		{
			await CleanUpAsync(tempFilePath, request.ObjectKey);
		}

		if (timeline.MapName is not { } mapName || !timeline.IsRadarCalibrated || !MapRadarSupport.HasVerifiedRadar(mapName))
		{
			return TacticErrors.DemoMapNotSupported;
		}

		return DemoNadeMapper.Map(mapName, timeline);
	}

	#endregion

	#region Private Methods

	private async Task CleanUpAsync(string tempFilePath, string objectKey)
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
			// Not tied to the request's token: a cancelled request must still not leave the demo behind in the bucket.
			await fileStorage.DeleteAsync(objectKey, CancellationToken.None);
		}
		catch (Exception)
		{
			// Best-effort cleanup — the bucket's lifecycle rule is the real backstop for anything this misses.
		}
	}

	#endregion
}
