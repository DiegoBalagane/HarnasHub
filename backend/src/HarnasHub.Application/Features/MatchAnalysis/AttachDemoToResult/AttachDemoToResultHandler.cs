#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Notifications;
using HarnasHub.Application.Features.MatchAnalysis.Shared;
using HarnasHub.Application.Features.Results.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.MatchAnalysis.AttachDemoToResult;

/// <summary>Handles <see cref="AttachDemoToResultCommand"/>: buffers the uploaded demo to a temp file (the parser needs a
/// seekable stream), parses the full timeline, recognises "us" from the roster or the recorded score, stores the
/// timeline for the match and deletes the demo from storage whatever happens.</summary>
public class AttachDemoToResultHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	IDemoParser demoParser,
	IRealtimeNotifier realtimeNotifier,
	TeamNotifications notifications,
	ILogger<AttachDemoToResultHandler> logger) : IRequestHandler<AttachDemoToResultCommand, ErrorOr<MatchDemoAnalysisDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<MatchDemoAnalysisDto>> Handle(AttachDemoToResultCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return ResultErrors.StorageNotConfigured;
		}

		var result = await dbContext.MatchResults.AsNoTracking().FirstOrDefaultAsync(r => r.Id == request.MatchResultId, cancellationToken);
		if (result is null)
		{
			await DeleteDemoAsync(request.ObjectKey);
			return ResultErrors.MatchNotFound;
		}

		var timeline = await ParseAsync(request.ObjectKey, cancellationToken);
		if (timeline is null || timeline.Rounds.Count == 0)
		{
			return ResultErrors.InvalidDemoFile;
		}

		var roster = await MatchTimelineAttacher.LoadRosterSteamIdsAsync(dbContext, cancellationToken);
		var ourTeam = TimelineTeamResolver.ResolveOurTeam(timeline.Rounds, roster, result.OurScore, result.OpponentScore);

		try
		{
			var analysis = await MatchTimelineAttacher.AttachAsync(
				dbContext, fileStorage, result.Id, timeline, DemoTimelineSerializer.CurrentParserVersion, ourTeam, cancellationToken);

			await realtimeNotifier.NotifyAsync($"match-analysis:{result.Id}", cancellationToken);
			// Re-attaching replaces the timeline, so the digest is posted again on purpose.
			await notifications.NotifyDemoReviewAsync(result.Id, cancellationToken);
			return new MatchDemoAnalysisDto(result.Id, analysis.RoundsCount, analysis.ParserVersion, ourTeam.Count > 0);
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, "Nie udało się zapisać osi czasu meczu {MatchResultId}", result.Id);
			return MatchAnalysisErrors.TimelineSaveFailed;
		}
	}

	#endregion

	#region Private Methods

	private async Task<DemoTimeline?> ParseAsync(string objectKey, CancellationToken cancellationToken)
	{
		var tempFilePath = Path.GetTempFileName();

		try
		{
			await using (var demoStream = await fileStorage.OpenReadAsync(objectKey, cancellationToken))
			await using (var tempFile = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				await demoStream.CopyToAsync(tempFile, cancellationToken);
			}

			await using var seekableStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
			return await demoParser.ParseAsync(seekableStream, DemoParseOptions.MatchAnalysis, cancellationToken);
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Nie udało się odczytać demki dołączanej do meczu (ObjectKey={ObjectKey})", objectKey);
			return null;
		}
		finally
		{
			try
			{
				File.Delete(tempFilePath);
			}
			catch (Exception)
			{
				// Best-effort — a stray temp file in a recycled container is harmless.
			}

			await DeleteDemoAsync(objectKey);
		}
	}

	private async Task DeleteDemoAsync(string objectKey)
	{
		try
		{
			// Not tied to the request's token: a cancelled request must still not leave the demo behind in the bucket.
			await fileStorage.DeleteAsync(objectKey, CancellationToken.None);
		}
		catch (Exception)
		{
			// Best-effort — the bucket's lifecycle rule is the backstop for anything this misses.
		}
	}

	#endregion
}
