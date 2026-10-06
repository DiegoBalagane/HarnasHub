using ErrorOr;
using HarnasHub.Application.Abstractions;
using HarnasHub.Application.Features.OpponentNotes.Shared;
using HarnasHub.Application.Features.OpponentReport.Shared;
using MediatR;

namespace HarnasHub.Application.Features.OpponentReport.GetOpponentReport;

/// <summary>Handles <see cref="GetOpponentReportQuery"/>. Never calls FACEIT — reading is free; only a sync spends API quota.</summary>
public class GetOpponentReportHandler(IApplicationDbContext dbContext, IFaceitClient faceitClient)
	: IRequestHandler<GetOpponentReportQuery, ErrorOr<OpponentReportDto>>
{
	#region Public Methods

	public async Task<ErrorOr<OpponentReportDto>> Handle(GetOpponentReportQuery request, CancellationToken cancellationToken)
	{
		var now = DateTime.UtcNow;
		var key = OpponentNames.ToKey(request.Name);

		var report = await OpponentReportSnapshots.LoadAsync(dbContext, key, cancellationToken)
			?? await OpponentReportGenerator.BuildAsync(dbContext, request.Name, now, cancellationToken);

		return await OpponentReportGenerator.WithLiveFieldsAsync(dbContext, report, faceitClient.IsConfigured, now, cancellationToken);
	}

	#endregion
}
