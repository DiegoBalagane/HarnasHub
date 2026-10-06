using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HarnasHub.Application.Features.OpponentReport.SyncOurFaceit;

/// <summary>Handles <see cref="SyncOurFaceitCommand"/> — no manual configuration: every user with a SteamID64 is looked up on FACEIT.</summary>
public class SyncOurFaceitHandler(
	IApplicationDbContext dbContext,
	IFaceitClient faceitClient,
	ILogger<SyncOurFaceitHandler> logger)
	: IRequestHandler<SyncOurFaceitCommand, ErrorOr<FaceitSyncResultDto>>
{
	#region Public Methods

	public async Task<ErrorOr<FaceitSyncResultDto>> Handle(SyncOurFaceitCommand request, CancellationToken cancellationToken)
	{
		if (!faceitClient.IsConfigured)
		{
			return OpponentReportErrors.FaceitNotConfigured;
		}

		try
		{
			var now = DateTime.UtcNow;
			var ourIds = await FaceitSync.ResolveOurPlayersAsync(dbContext, faceitClient, now, cancellationToken);
			var (newMapGames, complete) = await FaceitSync.SyncHistoryAsync(dbContext, faceitClient, ourIds, now, cancellationToken);
			return new FaceitSyncResultDto(ourIds.Count, newMapGames, complete);
		}
		catch (HttpRequestException ex)
		{
			logger.LogError(ex, "Synchronizacja FACEIT naszej drużyny nie powiodła się");
			return OpponentReportErrors.FaceitUnavailable;
		}
	}

	#endregion
}
