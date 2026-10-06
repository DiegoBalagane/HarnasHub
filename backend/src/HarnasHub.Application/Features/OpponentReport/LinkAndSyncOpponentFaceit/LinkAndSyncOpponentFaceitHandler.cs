#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentReport.LinkOpponentFaceit;
using HarnasHub.Application.Features.OpponentReport.Shared;
using HarnasHub.Application.Features.OpponentReport.SyncOpponentFaceit;
using MediatR;
using Microsoft.Extensions.Logging;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.LinkAndSyncOpponentFaceit;

/// <summary>Handles <see cref="LinkAndSyncOpponentFaceitCommand"/>: links first (a failure there is the job's failure), then
/// syncs without the manual-refresh cooldown. A failed sync keeps the link — the coach can refresh later — and is only logged.</summary>
public class LinkAndSyncOpponentFaceitHandler(
	ISender sender,
	IJobProgress jobProgress,
	ILogger<LinkAndSyncOpponentFaceitHandler> logger) : IRequestHandler<LinkAndSyncOpponentFaceitCommand, ErrorOr<OpponentFaceitLinkDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<OpponentFaceitLinkDto>> Handle(LinkAndSyncOpponentFaceitCommand request, CancellationToken cancellationToken)
	{
		jobProgress.Report(2, "Wyszukiwanie graczy na FACEIT");
		var linked = await sender.Send(new LinkOpponentFaceitCommand(request.OpponentName, request.Source), cancellationToken);
		if (linked.IsError)
		{
			return linked.Errors;
		}

		jobProgress.Report(10, "Powiązano — pobieranie meczów z FACEIT");
		var synced = await sender.Send(new SyncOpponentFaceitCommand(request.OpponentName, IsManual: false), cancellationToken);
		if (synced.IsError)
		{
			logger.LogWarning(
				"Powiązano przeciwnika {Opponent} z FACEIT, ale synchronizacja się nie udała: {Error}",
				request.OpponentName, synced.FirstError.Description);
			return linked.Value;
		}

		return synced.Value.Link ?? linked.Value;
	}

	#endregion
}
