using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;

/// <summary>Handles <see cref="SyncOpponentFaceitCommand"/>: refreshes the opponent's and our FACEIT cache, then rebuilds and
/// stores the report so the whole team sees the same snapshot.</summary>
public class SyncOpponentFaceitHandler(
	IApplicationDbContext dbContext,
	IFaceitClient faceitClient,
	IJobProgress jobProgress,
	ILogger<SyncOpponentFaceitHandler> logger)
	: IRequestHandler<SyncOpponentFaceitCommand, ErrorOr<OpponentReportDto>>
{
	#region Public Fields

	/// <summary>Minimum time between two manual refreshes of the same opponent.</summary>
	public static readonly TimeSpan ManualRefreshCooldown = TimeSpan.FromMinutes(10);

	#endregion

	#region Public Methods

	public async Task<ErrorOr<OpponentReportDto>> Handle(SyncOpponentFaceitCommand request, CancellationToken cancellationToken)
	{
		if (!faceitClient.IsConfigured)
		{
			return OpponentReportErrors.FaceitNotConfigured;
		}

		var key = OpponentNames.ToKey(request.OpponentName);
		var link = await dbContext.OpponentFaceitLinks.FirstOrDefaultAsync(l => l.OpponentKey == key, cancellationToken);
		if (link is null)
		{
			return OpponentReportErrors.NotLinked;
		}

		var now = DateTime.UtcNow;
		if (request.IsManual && link.LastSyncedAtUtc is { } lastSync && now - lastSync < ManualRefreshCooldown)
		{
			return OpponentReportErrors.RefreshTooSoon((int)Math.Ceiling((ManualRefreshCooldown - (now - lastSync)).TotalMinutes));
		}

		try
		{
			jobProgress.Report(5, "Pobieranie profili naszych graczy");
			var ourIds = await FaceitSync.ResolveOurPlayersAsync(dbContext, faceitClient, now, cancellationToken);
			jobProgress.Report(15, "Pobieranie profili graczy rywala");
			await FaceitSync.RefreshProfilesAsync(dbContext, faceitClient, link.PlayerIds, now, cancellationToken);
			jobProgress.BeginStep(25, 85, "Pobieranie historii meczów (to trwa najdłużej)");
			var (newMapGames, complete) = await FaceitSync.SyncHistoryAsync(
				dbContext, faceitClient, link.PlayerIds.Concat(ourIds).ToList(), now, cancellationToken, jobProgress.ReportStep);

			jobProgress.Report(85, "Pobieranie statystyk map (lifetime)");
			var activeIds = await OpponentReportGenerator.ActiveLineupIdsAsync(dbContext, link.FaceitTeamId, link.PlayerIds, now, cancellationToken);
			// Season players who aren't linked team members: their profile and history too, so their numbers aren't empty.
			var unlinked = activeIds.Except(link.PlayerIds).ToList();
			if (unlinked.Count > 0)
			{
				await FaceitSync.RefreshProfilesAsync(dbContext, faceitClient, unlinked, now, cancellationToken);
				await FaceitSync.SyncHistoryAsync(dbContext, faceitClient, unlinked, now, cancellationToken);
			}

			await FaceitLifetimeStats.RefreshAsync(dbContext, faceitClient, activeIds.Concat(ourIds).ToList(), now, cancellationToken);

			link.LastSyncedAtUtc = now;
			await dbContext.SaveChangesAsync(cancellationToken);
			logger.LogInformation(
				"Zsynchronizowano FACEIT dla {Opponent}: {NewMapGames} nowych map, komplet: {Complete}", link.DisplayName, newMapGames, complete);

			jobProgress.Report(90, "Budowanie raportu");
			var report = await OpponentReportGenerator.BuildAsync(dbContext, link.DisplayName, now, cancellationToken);
			await OpponentReportSnapshots.SaveAsync(dbContext, key, report, cancellationToken);
			await dbContext.SaveChangesAsync(cancellationToken);

			return await OpponentReportGenerator.WithLiveFieldsAsync(dbContext, report, true, now, cancellationToken);
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "Synchronizacja FACEIT dla przeciwnika {Opponent} nie powiodła się", link.DisplayName);
			return OpponentReportErrors.FaceitUnavailable;
		}
	}

	#endregion
}
