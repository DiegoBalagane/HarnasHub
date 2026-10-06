#region Usings

using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

#endregion

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentDemos;

/// <summary>Handles <see cref="GetOpponentDemosQuery"/> from the database only.</summary>
public class GetOpponentDemosHandler(
	IApplicationDbContext dbContext,
	IFileStorage fileStorage,
	IFaceitClient faceitClient,
	IFaceitDemoDownloader demoDownloader) : IRequestHandler<GetOpponentDemosQuery, ErrorOr<OpponentDemosDto>>
{
	#region Public Methods

	/// <inheritdoc />
	public async Task<ErrorOr<OpponentDemosDto>> Handle(GetOpponentDemosQuery request, CancellationToken cancellationToken)
	{
		var key = OpponentNames.ToKey(request.Name);
		var rows = await dbContext.OpponentDemoAnalyses.AsNoTracking()
			.Where(a => a.OpponentKey == key)
			.OrderByDescending(a => a.PlayedAtUtc ?? a.CreatedAtUtc)
			.ToListAsync(cancellationToken);

		var linked = await dbContext.OpponentFaceitLinks.AnyAsync(l => l.OpponentKey == key, cancellationToken);
		var autoDownload = fileStorage.IsConfigured && faceitClient.IsConfigured && demoDownloader.IsConfigured && linked;

		return new OpponentDemosDto(fileStorage.IsConfigured, autoDownload, await OpponentPlayerNames.ResolveDemosAsync(dbContext, rows.Select(OpponentDemoProcessor.ToDto).ToList(), cancellationToken));
	}

	#endregion
}
