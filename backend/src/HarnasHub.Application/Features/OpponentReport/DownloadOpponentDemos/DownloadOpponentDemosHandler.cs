#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Common.Notifications;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Core.Entities;
using HarnasHub.Core.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.DownloadOpponentDemos;

/// <summary>Handles <see cref="DownloadOpponentDemosCommand"/>: picks the newest cached FACEIT team games of the opponent
/// (same official-games + "≥ 3 of the lineup on one side" rule as the report) on the chosen maps that have no analysis yet, resolves each
/// map's <c>demo_url</c> from the match details, downloads it through the Downloads API and analyses it like an upload.
/// A failing demo is counted and skipped — the rest still go through.</summary>
public class DownloadOpponentDemosHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	IDemoParser demoParser,
	IFaceitClient faceitClient,
	IFaceitDemoDownloader demoDownloader,
	ICurrentUserService currentUser,
	IJobProgress jobProgress,
	TeamNotifications notifications,
	ILogger<DownloadOpponentDemosHandler> logger) : IRequestHandler<DownloadOpponentDemosCommand, ErrorOr<OpponentDemoDownloadResultDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<OpponentDemoDownloadResultDto>> Handle(DownloadOpponentDemosCommand request, CancellationToken cancellationToken)
	{
		if (!fileStorage.IsConfigured)
		{
			return OpponentDemoErrors.StorageNotConfigured;
		}

		if (!faceitClient.IsConfigured || !demoDownloader.IsConfigured)
		{
			return OpponentDemoErrors.DownloadsNotConfigured;
		}

		var key = OpponentNames.ToKey(request.OpponentName);
		var link = await dbContext.OpponentFaceitLinks.AsNoTracking().FirstOrDefaultAsync(l => l.OpponentKey == key, cancellationToken);
		if (link is null)
		{
			return OpponentReportErrors.NotLinked;
		}

		var candidates = await FindCandidatesAsync(key, link.FaceitTeamId, link.PlayerIds.ToHashSet(), request, cancellationToken);
		var stored = new List<OpponentDemoDto>();
		var failed = 0;
		var newMaps = new List<string>();

		foreach (var (match, index) in candidates.Select((m, i) => (m, i)))
		{
			var analysis = await DownloadAndStoreAsync(key, match, index, candidates.Count, cancellationToken);
			if (analysis is null)
			{
				failed++;
			}
			else
			{
				stored.Add(OpponentDemoProcessor.ToDto(analysis));
				if (analysis.FactsJson is not null)
				{
					newMaps.Add(analysis.MapName?.ToString() ?? string.Empty);
				}
			}
		}

		// One digest per batch, not per demo, so a download of several demos does not flood the channel.
		if (newMaps.Count > 0)
		{
			await notifications.NotifyOpponentDemosAsync(key, request.OpponentName, newMaps, cancellationToken);
		}

		return new OpponentDemoDownloadResultDto(stored.Count, failed, candidates.Count, await OpponentPlayerNames.ResolveDemosAsync(dbContext, stored, cancellationToken));
	}

	/// <summary>Newest team games of the opponent (<see cref="OpponentTeamGames"/>: official games of <paramref name="faceitTeamId"/>
	/// plus games of ≥ 3 of its lineup) on the requested maps without an analysis yet.</summary>
	public static List<FaceitMatch> SelectCandidates(
		IReadOnlyCollection<FaceitMatch> matches,
		IReadOnlySet<string> roster,
		IReadOnlyCollection<MapName>? maps,
		IReadOnlySet<(string MatchId, int MapNumber)> analysed,
		int count,
		string? faceitTeamId = null)
	{
		var teamRows = OpponentTeamGames.Select(matches, faceitTeamId, roster, DateTime.UtcNow).Games.Select(g => g.RowId).ToHashSet();
		return matches
			.Where(m => teamRows.Contains(m.Id))
			.Where(m => TeamMatchDetector.ParseMap(m.MapName) is { } map && (maps is null || maps.Count == 0 || maps.Contains(map)))
			.Where(m => !analysed.Contains((m.FaceitMatchId, m.MapNumber)))
			.OrderByDescending(m => m.PlayedAtUtc)
			.Take(count)
			.ToList();
	}

	#endregion

	#region Private Methods

	private async Task<List<FaceitMatch>> FindCandidatesAsync(
		string key, string? faceitTeamId, IReadOnlySet<string> roster, DownloadOpponentDemosCommand request, CancellationToken cancellationToken)
	{
		var since = DateTime.UtcNow - FaceitSync.HistoryWindow;
		var matches = await dbContext.FaceitMatches.AsNoTracking().Where(m => m.PlayedAtUtc >= since).ToListAsync(cancellationToken);
		var analysed = (await dbContext.OpponentDemoAnalyses.AsNoTracking()
				.Where(a => a.OpponentKey == key && a.FaceitMatchId != null)
				.Select(a => new { a.FaceitMatchId, a.FaceitMapNumber })
				.ToListAsync(cancellationToken))
			.Select(a => (a.FaceitMatchId!, a.FaceitMapNumber ?? 1))
			.ToHashSet();

		return SelectCandidates(matches, roster, request.Maps, analysed, request.Count, faceitTeamId);
	}

	private async Task<OpponentDemoAnalysis?> DownloadAndStoreAsync(
		string key, FaceitMatch match, int index, int total, CancellationToken cancellationToken)
	{
		var tempFilePath = Path.GetTempFileName();
		try
		{
			var info = await faceitClient.GetMatchAsync(match.FaceitMatchId, cancellationToken);
			var urls = info?.DemoUrls ?? [];
			var url = urls.Count >= match.MapNumber ? urls[match.MapNumber - 1] : urls.FirstOrDefault();
			if (url is null)
			{
				logger.LogWarning("Mecz FACEIT {MatchId} nie ma demki do pobrania", match.FaceitMatchId);
				return null;
			}

			// Each demo gets an equal slice of 5–95 %: the first third for the download, the rest for parsing.
			var from = 5 + (90 * index / total);
			var to = 5 + (90 * (index + 1) / total);
			var split = from + ((to - from) / 3);
			jobProgress.BeginStep(from, split, $"Demka {index + 1} z {total}: pobieranie z FACEIT");
			await demoDownloader.DownloadAsync(url, tempFilePath, cancellationToken);
			jobProgress.BeginStep(split, to, $"Demka {index + 1} z {total}: analiza");
			var timeline = await OpponentDemoReader.ParseFileAsync(demoParser, tempFilePath, logger, cancellationToken);
			if (timeline is null)
			{
				return null;
			}

			var input = new OpponentDemoInput(key, OpponentDemoSource.FaceitDownload, match.FaceitMatchId, match.MapNumber, match.PlayedAtUtc, currentUser.UserId);
			var analysis = await OpponentDemoProcessor.StoreAsync(dbContext, fileStorage, input, timeline, DateTime.UtcNow, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);
			return analysis;
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogWarning(ex, "Nie udało się pobrać ani przeanalizować demki FACEIT {MatchId} (mapa {MapNumber})", match.FaceitMatchId, match.MapNumber);
			return null;
		}
		finally
		{
			OpponentDemoReader.TryDeleteFile(tempFilePath);
		}
	}

	#endregion
}
